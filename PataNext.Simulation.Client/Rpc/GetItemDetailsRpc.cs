using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Injection;
using GameHost.Utility;
using PataNext.Game.GameItems;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components;
using StormiumTeam.GameBase;

namespace PataNext.Simulation.Client.Rpc
{
	public struct GetItemDetailsRpc : IGameHostRpcWithResponsePacket<GetItemDetailsRpc.Response>
	{
		public SerializableEntity ItemEntity;

		public struct Response : IGameHostRpcResponsePacket
		{
			public ResPath Asset;
			public string  AssetType;
			public string  Name;
			public string  Description;

			public string ItemType;
		}

		public class Process : RpcPacketWithResponseSystem<GetItemDetailsRpc, Response>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.GetItemDetails";

			protected override async ValueTask<Response> GetResponse(GetItemDetailsRpc request)
			{
				var app = GetClientAppUtility.Get(this);
				return await app.TaskScheduler.StartUnwrap(async () =>
				{
					var entity = (Entity)request.ItemEntity;
					if (entity.IsAlive == false)
						return await WithError(1, "Entity doesn't exist");

					Response response;

					var assetEntity = entity.Get<ItemInventory>().AssetEntity;
					var desc        = assetEntity.Get<GameItemDescription>();
					if (assetEntity.TryGet(out EquipmentItemDescription equipmentItemDescription))
						response.ItemType = equipmentItemDescription.ItemType;
					else
						response.ItemType = string.Empty;

					response.Asset = desc.Id;

					response.Name        = desc.Name;
					response.Description = desc.Description;
					response.AssetType   = desc.Type;

					return await WithResult(response);
				});
			}
		}
	}
}