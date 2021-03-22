using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using NotImplementedException = System.NotImplementedException;

namespace PataNext.Simulation.Client.Rpc
{
	public struct GetInventoryRpc : IGameHostRpcWithResponsePacket<GetInventoryRpc.Response>
	{
		/// <summary>
		/// If true, it will only search for categories inside <see cref="FilterCategories"/>.
		/// If false, it will search for all except categories inside <see cref="FilterCategories"/>
		/// </summary>
		public bool      FilterInclude;
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
				RequestUtility.Request<GetInventoryRequest> masterRequest;
				if (request.FilterInclude)
					masterRequest = new(World.Mgr, new(request.FilterCategories));
				else
					throw new NotImplementedException("exclude is not yet implemented");

				var response = await masterRequest.GetAsync<GetInventoryRequest.Response>();
				var array    = new Response.Item[response.ItemIds.Length];
				for (var i = 0; i != array.Length; i++)
				{
					var details = await RequestUtility.New(World.Mgr, new GetItemDetailsRequest(response.ItemIds[i]))
					                                  .GetAsync<GetItemDetailsRequest.Response>();

					array[i] = new()
					{
						Id           = new(response.ItemIds[i]),
						AssetResPath = details.ResPath,
						Name         = details.Name,
						Description  = details.Description,
						AssetType    = details.Type
					};
				}
				
				return await WithResult(new() {Items = array});
			}
		}
	}
}