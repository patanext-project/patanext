using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using PataNext.Module.Simulation.Network.MasterServer.Services.FullFledged;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.MasterServer.AssetService;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Hideout
{
	[UpdateAfter(typeof(SetLocalArmyFormationSystem))]
	public class UpdateMasterServerUnitSystem : GameAppSystem
	{
		private IScheduler initScheduler;

		private KitCollectionSystem                    kitCollectionSystem;
		private GameResourceDb<UnitArchetypeResource>  archDb;
		private GameResourceDb<UnitAttachmentResource> attachDb;
		private GameResourceDb<EquipmentResource>      equipDb;
		
		public UpdateMasterServerUnitSystem([NotNull] WorldCollection collection) : base(collection)
		{
			initScheduler = new Scheduler();
			
			DependencyResolver.Add(() => ref kitCollectionSystem);
			DependencyResolver.Add(() => ref archDb);
			DependencyResolver.Add(() => ref attachDb);
			DependencyResolver.Add(() => ref equipDb);
		}

		private EntityQuery uninitializedQuery;
		private EntityQuery unitQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			uninitializedQuery = CreateEntityQuery(new[]
			{
				AsComponentType<ArmyUnitDescription>(),
				AsComponentType<MasterServerControlledUnitData>()
			}, new[]
			{
				AsComponentType<Initialized>()
			});

			unitQuery = CreateEntityQuery(new[]
			{
				AsComponentType<Initialized>(),
				AsComponentType<UnitRequestManager>()
			});
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			uninitializedQuery.ForEachDeferred((ent) =>
			{
				var sourceGuid = GetComponentData<MasterServerControlledUnitData>(ent).UnitGuid
				                                                                      .ToString();

				var unitRequest = World.Mgr.CreateEntity();
				unitRequest.Set(new GetUnitDetailsRequest(sourceGuid));

				AddComponent(ent, new UnitRequestManager {Source   = unitRequest});
				AddComponent(ent, new Initialized());

				GameWorld.AssureComponents(ent, new[]
				{
					AsComponentType<UnitDisplayedEquipment>(),
					AsComponentType<UnitCurrentKit>(),
					AsComponentType<UnitArchetype>(),
					AsComponentType<UnitDefinedAbilities>()
				});
			}, initScheduler);

			initScheduler.Run();

			foreach (var unitHandle in unitQuery)
			{
				Entity presetRequest;

				var focus       = Focus(Safe(unitHandle));
				var unitRequest = GetComponentData<UnitRequestManager>(unitHandle).Source;

				if (unitRequest.TryGet(out GetUnitDetailsRequest.Response getUnitDetailsResponse))
				{
					var sourceGuid = getUnitDetailsResponse.Result.HardPresetId;

					presetRequest = World.Mgr.CreateEntity();
					presetRequest.Set(new GetAndSetFullUnitsPresetDetailsRequest {GameEntity = focus, SourceGuid = sourceGuid});
					/*presetRequest.Set(new GetUnitPresetDetailsRequest(sourceGuid));
					presetRequest.Set(new GetUnitPresetEquipmentsRequest(sourceGuid));
					presetRequest.Set(new GetUnitPresetAbilitiesRequest(sourceGuid));*/
					
					unitRequest.Remove<GetUnitDetailsRequest.Response>();

					initScheduler.Schedule(
						args => args.focus.AddData(new PresetRequestManager {Source = args.presetRequest}),
						(focus, presetRequest),
						default
					);
				}

				if (TryGetComponentData(unitHandle, out PresetRequestManager presetRequestManager))
					presetRequest = presetRequestManager.Source;
				else
					continue;

				/*if (presetRequest.TryGet(out GetUnitPresetDetailsRequest.Response getPresetDetailsResponse))
				{
					Console.WriteLine("received getPresetDetailsResponse");
					
					RequestUtility.CreateTracked(World.Mgr, new GetAssetPointerRequest(getPresetDetailsResponse.Result.ArchetypeId), (Entity _, GetAssetPointerRequest.Response response) =>
					{
						Console.WriteLine("received archetype of " + response.ResPath.FullString);
						GetComponentData<UnitArchetype>(unitHandle) = new(archDb.GetOrCreate(new(response.ResPath.FullString)));
					});
					RequestUtility.CreateTracked(World.Mgr, new GetAssetPointerRequest(getPresetDetailsResponse.Result.KitId), (Entity _, GetAssetPointerRequest.Response response) =>
					{
						var kitResource = kitCollectionSystem.GetKit(response.ResPath);
						GetComponentData<UnitCurrentKit>(unitHandle) = new(kitResource);
					});

					presetRequest.Remove<GetUnitPresetDetailsRequest.Response>();
				}*/

				/*if (presetRequest.TryGet(out GetUnitPresetEquipmentsRequest.Response getPresetEquipmentsResponse))
				{
					var equipBuffer = GetBuffer<UnitDisplayedEquipment>(unitHandle);
					equipBuffer.Clear();

					Console.WriteLine("received equipment!");
					
					foreach (var (rootAssetId, itemId) in getPresetEquipmentsResponse.Result)
					{
						RequestUtility.CreateTracked(World.Mgr, new GetAssetPointerRequest(rootAssetId), (Entity _, GetAssetPointerRequest.Response rootR) =>
						{
							RequestUtility.CreateTracked(World.Mgr, new GetItemAssetPointerRequest(itemId), (Entity _, GetItemAssetPointerRequest.Response itemR) =>
							{
								equipBuffer.Add(new()
								{
									Attachment = attachDb.GetOrCreate(rootR.ResPath.FullString),
									Resource   = equipDb.GetOrCreate(itemR.ResPath.FullString)
								});
							});
						});
					}

					presetRequest.Remove<GetUnitPresetEquipmentsRequest.Response>();
				}*/
				
				/*if (presetRequest.TryGet(out GetUnitPresetAbilitiesRequest.Response getPresetAbilitiesResponse))
				{
					var abilityBuffer = GetBuffer<UnitDefinedAbilities>(unitHandle);
					abilityBuffer.Clear();

					Console.WriteLine("received abilities!");
					
					foreach (var (_, abilityMap) in getPresetAbilitiesResponse.Result)
					{
						foreach (var (songAsset, view) in abilityMap)
						{
							RequestUtility.CreateTracked(World.Mgr, new GetAssetPointerRequest(songAsset), (Entity _, GetAssetPointerRequest.Response songR) =>
							{
								if (!string.IsNullOrEmpty(view.Top))
									RequestUtility.CreateTracked(World.Mgr, new GetAssetPointerRequest(view.Top), (Entity _, GetAssetPointerRequest.Response response) => { abilityBuffer.Add(new(response.ResPath.FullString, AbilitySelection.Top)); });

								if (!string.IsNullOrEmpty(view.Mid))
									RequestUtility.CreateTracked(World.Mgr, new GetAssetPointerRequest(view.Mid), (Entity _, GetAssetPointerRequest.Response response) =>
									{
										Console.WriteLine("added ability: " + response.ResPath.FullString);
										abilityBuffer.Add(new(response.ResPath.FullString, AbilitySelection.Horizontal));
									});

								if (!string.IsNullOrEmpty(view.Bot))
									RequestUtility.CreateTracked(World.Mgr, new GetAssetPointerRequest(view.Bot), (Entity _, GetAssetPointerRequest.Response response) => { abilityBuffer.Add(new(response.ResPath.FullString, AbilitySelection.Bottom)); });
							});
						}
					}

					presetRequest.Remove<GetUnitPresetAbilitiesRequest.Response>();
				}*/
				
				
			}

			initScheduler.Run();
		}

		public struct Initialized : IComponentData
		{
		}

		public struct UnitRequestManager : IComponentData
		{
			public Entity Source;
		}

		public struct PresetRequestManager : IComponentData
		{
			public Entity Source;
		}
	}
}