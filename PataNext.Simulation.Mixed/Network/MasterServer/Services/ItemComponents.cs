using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using PataNext.MasterServerShared.Services;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.MasterServer;

namespace PataNext.Module.Simulation.Network.MasterServer.Services
{
	public struct GetItemAssetPointerRequest
	{
		public string ItemGuid;

		public GetItemAssetPointerRequest(string itemGuid) => ItemGuid = itemGuid;

		public struct Response
		{
			public string Author => ResPath.Author;
			public string Mod    => ResPath.ModPack;
			public string Id     => ResPath.Resource;

			public ResPath ResPath;
		}

		public class Process : MasterServerRequestHub<IItemHub, IItemHubReceiver, GetItemAssetPointerRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var result = await Service.GetAssetPointer(entity.Get<GetItemAssetPointerRequest>().ItemGuid);
				return e => e.Set(new Response
				{
					ResPath = new(ResPath.EType.MasterServer, result.Author, result.Mod, result.Id)
				});
			}
		}
	}
	
	public struct GetItemDetailsRequest
	{
		public string ItemGuid;

		public GetItemDetailsRequest(string itemGuid) => ItemGuid = itemGuid;

		public struct Response
		{
			public ResPath ResPath;
			public ResPath Type;
			public string  Name, Description;

			public int StackCount;
		}

		public class Process : MasterServerRequestHub<IItemHub, IItemHubReceiver, GetItemDetailsRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var result = await Service.GetItemDetails(entity.Get<GetItemDetailsRequest>().ItemGuid);

				return e => e.Set(new Response
				{
					ResPath     = new(ResPath.EType.MasterServer, result.AssetDetails.Pointer.Author, result.AssetDetails.Pointer.Mod, result.AssetDetails.Pointer.Id),
					Name        = result.AssetDetails.Name,
					Description = result.AssetDetails.Description,
					Type        = new(result.AssetDetails.Type),

					StackCount = result.StackCount
				});
			}
		}
	}

	public struct GetInventoryRequest
	{
		public string[] AssetTypes;
		public string   SaveId;

		public GetInventoryRequest(string saveId, string[] assetTypes)
		{
			SaveId     = saveId;
			AssetTypes = assetTypes;
		}

		public struct Response
		{
			public string[] ItemIds;
		}

		public class Process : MasterServerRequestHub<IItemHub, IItemHubReceiver, GetInventoryRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var req    = entity.Get<GetInventoryRequest>();
				var result = await Service.GetInventory(req.SaveId, req.AssetTypes);
				return e => e.Set(new Response
				{
					ItemIds = result
				});
			}
		}
	}
}