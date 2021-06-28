using System;
using System.Threading.Tasks;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Core.Threading;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Utility;
using PataNext.Game.Rpc.SerializationUtility;
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
					if (string.IsNullOrEmpty(request.Save.Value))
					{
						if (GetClientAppUtility.GetLocalPlayerSave(app) is { } localPlayerSave)
							request.Save = new(localPlayerSave);
						else
							return await WithError(1, "couldn't resolve local save");
					}

					if (request.Save.Equals(default))
						return await WithError(1, "couldn't resolve local save");
					
					RequestUtility.Request<GetInventoryRequest> masterRequest;
					if (request.FilterInclude)
						masterRequest = new(app.Data.World, new(request.Save.Value, request.FilterCategories));
					else
						throw new NotImplementedException("exclude is not yet implemented");
					
					Console.WriteLine("inventory {1} " + request.Save.Value);

					var response = await masterRequest.GetAsync<GetInventoryRequest.Response>();

					Console.WriteLine("inventory {2}");

					var innerRequests = new Task<GetItemDetailsRequest.Response>[response.ItemIds.Length];
					var array         = new Response.Item[response.ItemIds.Length];
					for (var i = 0; i != array.Length; i++)
					{
						innerRequests[i] = RequestUtility.New(app.Data.World, new GetItemDetailsRequest(response.ItemIds[i]))
						                                 .GetAsync<GetItemDetailsRequest.Response>();
					}
					
					Console.WriteLine("inventory {3}");

					for (var i = 0; i != array.Length; i++)
					{
						var details = await innerRequests[i];
						array[i] = new()
						{
							Id           = new(response.ItemIds[i]),
							AssetResPath = details.ResPath,
							Name         = details.Name,
							Description  = details.Description,
							AssetType    = details.Type.FullString
						};
					}
					
					Console.WriteLine("inventory {4} + " + array.Length);

					return await WithResult(new() {Items = array});
				});
			}
		}
	}
}