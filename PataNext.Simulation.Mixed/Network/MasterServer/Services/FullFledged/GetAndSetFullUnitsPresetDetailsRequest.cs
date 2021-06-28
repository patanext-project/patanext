using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Injection.Dependency;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using GameHost.Utility;
using JetBrains.Annotations;
using MagicOnion.Client;
using PataNext.MasterServerShared.Services;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using STMasterServer.Shared.Services.Assets;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.AssetService;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
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

			public Process([NotNull] WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref unitHub);
				DependencyResolver.Add(() => ref unitPresetHub);
				DependencyResolver.Add(() => ref itemHub);

				DependencyResolver.Add(() => ref attachDb);
				DependencyResolver.Add(() => ref equipDb);
				DependencyResolver.Add(() => ref archDb);
				DependencyResolver.Add(() => ref kitCollectionSystem);
			}

			private IViewableAssetService                                            viewableAssetService;
			private HubClientConnectionCache<IUnitHub, IUnitHubReceiver>             unitHub;
			private HubClientConnectionCache<IUnitPresetHub, IUnitPresetHubReceiver> unitPresetHub;
			private HubClientConnectionCache<IItemHub, IItemHubReceiver>             itemHub;
			
			protected override void OnFeatureAdded(Entity entity, MasterServerFeature obj)
			{
				base.OnFeatureAdded(entity, obj);

				viewableAssetService = MagicOnionClient.Create<IViewableAssetService>(obj.Channel);

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
				var equipmentMap   = await unitPresetClient.GetEquipments(presetId);
				var equipmentFinal = new UnitDisplayedEquipment[equipmentMap.Count];
				
				{
					var count = 0;
					var tasks = new List<Task<(string attachment, string resource)>>();
					foreach (var (rootAssetId, itemId) in equipmentMap)
					{
						tasks.Add(Task.Run(async () =>
							(
								(await viewableAssetService.GetPointer(rootAssetId)).ToResPath().FullString,
								(await itemClient.GetAssetPointer(itemId)).ToResPath().FullString
							)
						));
					}

					globalTasks.Add(TaskScheduler.StartUnwrap(async () =>
					{
						foreach (var t in tasks)
						{
							var (attachment, resource) = await t;
							equipmentFinal[count++] = new()
							{
								Attachment = attachDb.GetOrCreate(attachment),
								Resource   = equipDb.GetOrCreate(resource)
							};
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

					var displayedEquipments = target.GetBuffer<UnitDisplayedEquipment>();
					displayedEquipments.Clear();
					displayedEquipments.AddRange(equipmentFinal);

					var definedEquipments = target.GetBuffer<UnitDefinedEquipments>();
					definedEquipments.Clear();
					definedEquipments.Reinterpret<UnitDisplayedEquipment>()
					                 .AddRange(equipmentFinal);

					var definedAbilities = target.GetBuffer<UnitDefinedAbilities>();
					definedAbilities.Clear();
					definedAbilities.AddRange(CollectionsMarshal.AsSpan(abilityList));

					target.AddData(new MasterServerIsUnitLoaded());

					Console.WriteLine($"Success for {e}");
				};
			}
		}
	}
}