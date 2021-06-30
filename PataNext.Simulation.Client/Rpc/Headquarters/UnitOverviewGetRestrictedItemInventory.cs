using System;
using System.Threading.Tasks;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using GameHost.Utility;
using PataNext.Game.GameItems;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Simulation.Client.Rpc
{
	public struct UnitOverviewGetRestrictedItemInventory : IGameHostRpcWithResponsePacket<UnitOverviewGetRestrictedItemInventory.Response>
	{
		public GameEntity EntityTarget;
		public string     AttachmentTarget;

		public struct Response : IGameHostRpcResponsePacket
		{
			public struct Item
			{
				public SerializableEntity Id;
				public ResPath            AssetResPath;

				public string             AssetType;
				public string             Name;
				public string             Description;
				public MasterServerUnitId EquippedBy;
			}

			public Item[] Items;
		}

		public class Process : RpcPacketWithResponseSystem<UnitOverviewGetRestrictedItemInventory, Response>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.Client.UnitOverview.GetRestrictedItemInventory";

			protected override async ValueTask<Response> GetResponse(UnitOverviewGetRestrictedItemInventory request)
			{
				var app = GetClientAppUtility.Get(this);
				Console.WriteLine("inventory wanted!");

				return await app.TaskScheduler.StartUnwrap(async () =>
				{
					var player = GetClientAppUtility.GetLocalPlayer(app);
					if (!player.Exists())
						return await WithError(1, "local player not found");
					if (!player.Has<PlayerInventoryTarget>())
						return await WithError(1, "inventory not found on player");

					var unit = new SafeEntityFocus(player.GameWorld, request.EntityTarget);
					if (!unit.Exists())
						return await WithError(2, "unit doesn't exist");
					if (!unit.Has<UnitAllowedEquipment>())
						return await WithError(3, "unit has no UnitAllowedEquipment buffer");

					var gameWorld = unit.GameWorld;

					using var contains = new PooledList<ResPath>();
					foreach (var allowed in unit.GetBuffer<UnitAllowedEquipment>())
					{
						if (new ResPath(gameWorld.GetComponentData<UnitAttachmentResource>(allowed.Attachment.Handle).Value.ToString()).Equals(new(request.AttachmentTarget)))
							contains.Add(new(gameWorld.GetComponentData<EquipmentResource>(allowed.EquipmentType.Handle).Value.ToString()));
					}

					foreach (var t in contains)
						Console.WriteLine(">>> " + t.FullString);

					var inventory = player.GetData<PlayerInventoryTarget>().Value
					                      .Get<PlayerInventoryBase>();

					var itemMgr = new ContextBindingStrategy(app.Data.Context, true).Resolve<GameItemsManager>();

					using var list = new PooledList<Entity>();
					inventory.Read(list);

					using var final = new PooledList<Response.Item>();
					foreach (var item in list)
					{
						var asset = item.Get<TrucItemInventory>().AssetEntity;
						if (!asset.TryGet(out EquipmentItemDescription equipmentDesc))
							continue;

						var desc = asset.Get<GameItemDescription>();
						Console.WriteLine("<<< " + new ResPath(equipmentDesc.ItemType).FullString + ", " + desc.Type + contains.Contains(new(equipmentDesc.ItemType)));

						if (contains.Contains(new(equipmentDesc.ItemType)))
							final.Add(new()
							{
								Id           = item,
								AssetResPath = desc.Id,
								AssetType    = desc.Type,
								Name         = desc.Name,
								Description  = desc.Description,
								EquippedBy   = default
							});
					}

					Console.WriteLine("yes");
					return await WithResult(new() { Items = final.ToArray() });
				});
			}
		}
	}
}