using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using GameHost.Core.Threading;
using GameHost.Injection.Dependency;
using PataNext.MasterServerShared.Services;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.Utility;

namespace PataNext.Module.Simulation.Network.MasterServer.Systems
{
	public class SynchronizeInventorySystem : AppSystemWithFeature<MasterServerFeature>
	{
		private ItemHubReceiver itemHubReceiver;

		private IScheduler scheduler;

		public SynchronizeInventorySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref itemHubReceiver);

			AddDisposable(inventoryReqEntity = World.Mgr.CreateEntity());
			AddDisposable(scheduler          = new Scheduler());
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			itemHubReceiver.InventoryUpdate += () => inventoryUpdate = true;
			itemHubReceiver.ItemUpdate      += guid => itemUpdateSet.Add(guid);
		}

		public override bool CanUpdate()
		{
			var isEmpty = World.Mgr.Get<MasterServerPlayerInventory>().IsEmpty;
			if (isEmpty)
				inventoryUpdate = true;

			return base.CanUpdate() && isEmpty == false;
		}

		private bool inventoryUpdate = true;

		// This set contains items that are known from the system, aka all information were get
		private HashSet<string> knownItemSet = new();

		// This set contains items that were updated, once the information is get, it will be put into knownItemSet
		private HashSet<string> itemUpdateSet = new();

		private MasterServerPlayerInventory previousInventory; // inventoryUpdate will be set to true if the inventory is different
		private MasterServerPlayerInventory getInventory() => World.Mgr.Get<MasterServerPlayerInventory>()[0];

		// Entity used for GetInventory requests
		// (note: requests for items are done on their own entities)
		private Entity inventoryReqEntity;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var inventory = getInventory();
			if (previousInventory != inventory)
			{
				previousInventory = inventory;
				inventoryUpdate   = true;
			}

			if (inventoryUpdate)
			{
				inventoryUpdate = false;
				
				inventoryReqEntity.Set(new GetInventoryRequest(inventory.SaveId, Array.Empty<string>()));
				inventoryReqEntity.Remove<GetInventoryRequest.Response>();
			}

			if (inventoryReqEntity.TryGet(out GetInventoryRequest.Response inventoryResponse))
			{
				inventoryReqEntity.Remove<GetInventoryRequest.Response>();

				inventory.setMasterServerItems(createEntity, inventoryResponse.ItemIds);
			}

			foreach (var updateGuid in itemUpdateSet)
			{
				if (!inventory.msIdToEntity.TryGetValue(updateGuid, out var entity))
					continue;

				if (entity.TryGet(out GetItemDetailsRequest.Response response))
				{
					entity.Remove<GetItemDetailsRequest.Response>();
					
					inventory.setKnownItem(updateGuid, new()
					{
						AdditionalData = entity,
						AssetId        = response.ResPath,
						TypeId         = response.Type,

						Count       = response.StackCount,
						IsStackable = response.StackCount > 0
					});
					
					scheduler.Schedule(args => args.set.Remove(args.guid), (set: itemUpdateSet, guid: updateGuid), default);
				}
				else if (!entity.Has<GetItemDetailsRequest>())
				{
					entity.Set(new GetItemDetailsRequest(updateGuid));
				}
			}
			
			scheduler.Run();
		}


		private Func<string, Entity> createEntity;

		protected override void OnInit()
		{
			base.OnInit();

			createEntity = guid =>
			{
				var entity = World.Mgr.CreateEntity();
				entity.Set(new GetItemDetailsRequest(guid));
				
				itemUpdateSet.Add(guid);

				return entity;
			};
		}
	}
}