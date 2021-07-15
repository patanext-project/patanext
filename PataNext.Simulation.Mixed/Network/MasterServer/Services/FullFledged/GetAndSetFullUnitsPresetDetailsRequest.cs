using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Injection.Dependency;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using GameHost.Utility;
using JetBrains.Annotations;
using MagicOnion.Client;
using PataNext.Game.GameItems;
using PataNext.MasterServerShared.Services;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Network.MasterServer.Systems;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using STMasterServer.Shared.Services.Assets;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.AssetService;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Network.MasterServer.Services.FullFledged
{
	public struct GetAndSetFullUnitsPresetDetailsRequest
	{
		public SafeEntityFocus GameEntity;

		public string          SourceGuid;

		public class Process : MasterServerRequestServiceMarkerDefaultEcs<GetAndSetFullUnitsPresetDetailsRequest>
		{
			private GameResourceDb<UnitAttachmentResource> attachDb;
			private GameResourceDb<EquipmentResource>      equipDb;
			private GameResourceDb<UnitArchetypeResource>  archDb;
			private KitCollectionSystem                    kitCollectionSystem;
			private GameItemsManager                       itemsManager;

			private ResPathGen resPathGen;

			public Process([NotNull] WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref unitHub);
				DependencyResolver.Add(() => ref unitPresetHub);
				DependencyResolver.Add(() => ref itemHub);

				DependencyResolver.Add(() => ref attachDb);
				DependencyResolver.Add(() => ref equipDb);
				DependencyResolver.Add(() => ref archDb);
				DependencyResolver.Add(() => ref kitCollectionSystem);
				DependencyResolver.Add(() => ref itemsManager);
				
				DependencyResolver.Add(() => ref resPathGen);
			}

			private IViewableAssetService                                            viewableAssetService;
			private IRoleService                                            roleService;
			private HubClientConnectionCache<IUnitHub, IUnitHubReceiver>             unitHub;
			private HubClientConnectionCache<IUnitPresetHub, IUnitPresetHubReceiver> unitPresetHub;
			private HubClientConnectionCache<IItemHub, IItemHubReceiver>             itemHub;
			
			protected override void OnFeatureAdded(Entity entity, MasterServerFeature obj)
			{
				base.OnFeatureAdded(entity, obj);

				viewableAssetService = MagicOnionClient.Create<IViewableAssetService>(obj.Channel);
				roleService          = MagicOnionClient.Create<IRoleService>(obj.Channel);

				DependencyResolver.AddDependency(new TaskDependency(unitHub.Client));
				DependencyResolver.AddDependency(new TaskDependency(unitPresetHub.Client));
				DependencyResolver.AddDependency(new TaskDependency(itemHub.Client));
			}
			
			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				if (!(unitPresetHub.Client.Result is { } unitPresetClient))
					throw new NullReferenceException(nameof(unitPresetHub));
				if (!(itemHub.Client.Result is { } itemClient))
					throw new NullReferenceException(nameof(unitPresetHub));

				var focus     = entity.Get<GetAndSetFullUnitsPresetDetailsRequest>().GameEntity;
				var player    = focus.GetData<Relative<PlayerDescription>>();
				var inventory = focus.GameWorld.GetComponentData<PlayerInventoryTarget>(player.Handle).Value.Get<MasterServerPlayerInventory>();

				var presetId = entity.Get<GetAndSetFullUnitsPresetDetailsRequest>().SourceGuid;
				var globalTasks = new List<Task>();

				// ----------------- ---------------- ++
				//	Preset Details
				// ++ -------------- ++
				var presetDetails     = await unitPresetClient.GetDetails(presetId);
				var kitAssetPtr       = await viewableAssetService.GetPointer(presetDetails.KitId);
				var archetypeAssetPtr = await viewableAssetService.GetPointer(presetDetails.ArchetypeId);

				// ----------------- ---------------- ++
				//	Equipments
				// ++ -------------- ++
				var equipmentMap        = await unitPresetClient.GetEquipments(presetId);
				var allowedEquipmentMap = await roleService.GetAllowedEquipments(presetDetails.RoleId);
				
				var equipmentFinal     = new UnitDisplayedEquipment[equipmentMap.Count];
				var equipmentItemFinal = new UnitDefinedEquipments[equipmentMap.Count];
				var allowedEquipmentFinal   = new PooledList<UnitAllowedEquipment>();
				
				{
					var count   = 0;
					var tasks   = new List<Task<(string attachment, string resource, string itemId)>>();
					var aeTasks = new List<Task<(string attachment, string[] types)>>();
					foreach (var (rootAssetId, itemId) in equipmentMap)
					{
						tasks.Add(Task.Run(async () =>
							(
								(await viewableAssetService.GetPointer(rootAssetId)).ToResPath().FullString,
								(await itemClient.GetAssetPointer(itemId)).ToResPath().FullString,
								itemId
							)
						));
					}

					foreach (var (rootAssetId, allowedTypeIds) in allowedEquipmentMap)
					{
						aeTasks.Add(Task.Run(async () =>
						{
							var array = new string[allowedTypeIds.Length];
							for (var i = 0; i < allowedTypeIds.Length; i++)
								array[i] = (await viewableAssetService.GetPointer(allowedTypeIds[i])).ToResPath().FullString;

							return (
								(await viewableAssetService.GetPointer(rootAssetId)).ToResPath().FullString,
								array
							);
						}));
					}

					globalTasks.Add(TaskScheduler.StartUnwrap(async () =>
					{
						foreach (var t in tasks)
						{
							var (attachment, resource, itemId) = await t;
							if (!itemsManager.TryGetDescription(new(resource), out var itemAsset))
								throw new InvalidOperationException("no asset found on " + resource);

							if (!itemAsset.Has<GameItemDescription>())
								throw new InvalidOperationException("no desc on " + resource);
							
							var i = count++;
							equipmentFinal[i] = new()
							{
								Attachment = attachDb.GetOrCreate(attachment),
								Resource   = equipDb.GetOrCreate(resource)
							};
							equipmentItemFinal[i] = new()
							{
								Attachment = attachDb.GetOrCreate(attachment),
								Item       = inventory.getOrCreateTemporaryItem(itemId, itemAsset)
							};
						}

						foreach (var t in aeTasks)
						{
							var (attachment, types) = await t;
							foreach (var type in types)
								allowedEquipmentFinal.Add(new()
								{
									Attachment    = attachDb.GetOrCreate(attachment),
									EquipmentType = equipDb.GetOrCreate(type)
								});
						}
					}));
				}

				// ----------------- ---------------- ++
				//	Abilities
				// ++ -------------- ++
				var abilitySongMap = await unitPresetClient.GetAbilities(presetId);
				var abilityList    = new List<UnitDefinedAbilities>(); // we don't know how much abilities will be there

				{
					var tasks = new List<Task<UnitDefinedAbilities>>();
					foreach (var (_, abilityMap) in abilitySongMap)
					{
						foreach (var (songAsset, view) in abilityMap)
						{
							await viewableAssetService.GetPointer(songAsset);

							if (!string.IsNullOrEmpty(view.Top))
								tasks.Add(Task.Run(async () => new UnitDefinedAbilities((await viewableAssetService.GetPointer(view.Top)).ToResPath().FullString, AbilitySelection.Top)));
							if (!string.IsNullOrEmpty(view.Mid))
								tasks.Add(Task.Run(async () => new UnitDefinedAbilities((await viewableAssetService.GetPointer(view.Mid)).ToResPath().FullString, AbilitySelection.Horizontal)));
							if (!string.IsNullOrEmpty(view.Bot))
								tasks.Add(Task.Run(async () => new UnitDefinedAbilities((await viewableAssetService.GetPointer(view.Bot)).ToResPath().FullString, AbilitySelection.Bottom)));
						}
					}

					globalTasks.Add(Task.Run(async () =>
					{
						foreach (var t in tasks)
							abilityList.Add(await t);
					}));
				}
				
				foreach (var t in globalTasks)
					await t;

				return e =>
				{
					var target = e.Get<GetAndSetFullUnitsPresetDetailsRequest>().GameEntity;
					target.GetData<UnitArchetype>()  = new(archDb.GetOrCreate(new(archetypeAssetPtr.ToResPath().FullString)));
					target.GetData<UnitCurrentKit>() = new(kitCollectionSystem.GetKit(kitAssetPtr.ToResPath()));

					var maskFound = false;
					foreach (var elem in equipmentFinal)
					{
						if (target.GameWorld.GetComponentData<UnitAttachmentResource>(elem.Attachment.Handle).Value.Span.IndexOf("mask") >= 0)
						{
							maskFound = true;
							break;
						}
					}

					var displayedEquipments = target.GetBuffer<UnitDisplayedEquipment>();
					displayedEquipments.Clear();
					displayedEquipments.AddRange(equipmentFinal);
					if (!maskFound)
					{
						var maskAttachment = attachDb.GetOrCreate(new(resPathGen.Create(new[] { "equip_root", "mask" }, ResPath.EType.MasterServer)));
						var kit            = target.GetData<UnitCurrentKit>().Resource;
						var resource       = target.GameWorld.GetComponentData<UnitKitResource>(kit.Handle).Value.Span;
						resource = resource[(resource.LastIndexOf('/')+1)..];
						
						var mask = equipDb.GetOrCreate(new(resPathGen.Create(new[] { "equipment", "mask", resource.ToString() }, ResPath.EType.MasterServer)));
						displayedEquipments.Add(new(maskAttachment, mask));
					}
					
					var definedEquipments = target.GetBuffer<UnitDefinedEquipments>();
					definedEquipments.Clear();
					definedEquipments.AddRange(equipmentItemFinal);

					var allowedEquipments = target.GetBuffer<UnitAllowedEquipment>();
					allowedEquipments.Clear();
					allowedEquipments.AddRange(allowedEquipmentFinal.Span);

					var definedAbilities = target.GetBuffer<UnitDefinedAbilities>();
					definedAbilities.Clear();
					definedAbilities.AddRange(CollectionsMarshal.AsSpan(abilityList));

					target.AddData(new MasterServerIsUnitLoaded());
					target.AddData(new MasterServerUnitPresetData(presetId));
				};
			}
		}
	}
}