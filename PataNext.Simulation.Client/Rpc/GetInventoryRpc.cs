using System;
using System.Threading.Tasks;
using Collections.Pooled;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Utility;
using PataNext.Game.GameItems;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using NotImplementedException = System.NotImplementedException;

namespace PataNext.Simulation.Client.Rpc
{
	public struct GetInventoryRpc : IGameHostRpcWithResponsePacket<GetInventoryRpc.Response>
	{
		public MasterServerSaveId Save; // optional

		/// <summary>
		/// If true, it will only search for categories inside <see cref="FilterCategories"/>.
		/// If false, it will search for all except categories inside <see cref="FilterCategories"/>
		/// </summary>
		public bool FilterInclude;

		public string[] FilterCategories;

		public struct Response : IGameHostRpcResponsePacket
		{
			public struct Item
			{
				public MasterServerItemId Id;
				public ResPath            AssetResPath;

				public string             AssetType;
				public string             Name;
				public string             Description;
				public MasterServerUnitId EquippedBy;
			}

			public Item[] Items;
		}

		public class System : RpcPacketWithResponseSystem<GetInventoryRpc, Response>
		{
			public System(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.GetInventory";

			protected override async ValueTask<Response> GetResponse(GetInventoryRpc request)
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

					var inventory = player.GetData<PlayerInventoryTarget>().Value
					                      .Get<PlayerInventoryBase>();

					var itemMgr = new ContextBindingStrategy(app.Data.Context, true).Resolve<GameItemsManager>();

					using var list = new PooledList<InventoryItem>();
					inventory.Read(list, request.FilterCategories);

					var array = new Response.Item[list.Count];
					for (var i = 0; i != list.Count; i++)
					{
						var item = list[i];
						var id   = string.Empty;
						if (item.ItemTarget.IsAlive && item.ItemTarget.TryGet(out MasterServerControlledItemData controlledItemData))
							id = controlledItemData.ItemGuid.ToString();

						var name        = "null name";
						var description = "null description";
						if (itemMgr.TryGetDescription(item.AssetId, out var entityDesc))
						{
							var desc = entityDesc.Get<GameItemDescription>();
							name        = desc.Name;
							description = desc.Description;
						}

						array[i] = new()
						{
							Id           = new(id),
							AssetResPath = item.AssetId,
							Name         = name,
							Description  = description,
							AssetType    = item.TypeId.FullString
						};
					}

					Console.WriteLine("inventory {4} + " + array.Length);

					return await WithResult(new() { Items = array });
				});
			}
		}
	}
}