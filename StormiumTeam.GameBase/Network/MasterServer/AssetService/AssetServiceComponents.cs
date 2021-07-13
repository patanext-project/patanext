using System;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using STMasterServer.Shared.Services.Assets;

namespace StormiumTeam.GameBase.Network.MasterServer.AssetService
{
	public struct GetAssetPointerRequest
	{
		public string AssetGuid;

		public GetAssetPointerRequest(string assetGuid)
		{
			AssetGuid = assetGuid;
		}

		public struct Response
		{
			public string Author => ResPath.Author;
			public string Mod    => ResPath.ModPack;
			public string Id     => ResPath.Resource;

			public ResPath ResPath;
		}

		public class Process : MasterServerRequestService<IViewableAssetService, GetAssetPointerRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var result = await Service.GetPointer(entity.Get<GetAssetPointerRequest>().AssetGuid);

				return e => e.Set(new Response
				{
					ResPath = new(ResPath.EType.MasterServer, result.Author, result.Mod, result.Id)
				});
			}
		}
	}

	public struct GetAssetDetailsRequest
	{
		public string AssetGuid;

		public GetAssetDetailsRequest(string assetGuid)
		{
			AssetGuid = assetGuid;
		}

		public struct Response
		{
			public ResPath ResPath;
			public string  Name, Description;
			public string  Type;
		}

		public class Process : MasterServerRequestService<IViewableAssetService, GetAssetPointerRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var result = await Service.GetDetails(entity.Get<GetAssetPointerRequest>().AssetGuid);

				return e => e.Set(new Response
				{
					ResPath     = new(ResPath.EType.MasterServer, result.Pointer.Author, result.Pointer.Mod, result.Pointer.Author),
					Name        = result.Name,
					Description = result.Description,
					Type        = result.Type
				});
			}
		}
	}

	public struct GetAssetGuidRequest
	{
		public ResPath Path;

		public struct Response
		{
			public string Guid;
		}

		public class Process : MasterServerRequestService<IViewableAssetService, GetAssetGuidRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var path   = entity.Get<GetAssetGuidRequest>().Path;
				var result = await Service.GetGuid(path.Author, path.ModPack, path.Resource);

				return e => e.Set(new Response
				{
					Guid = result
				});
			}
		}
	}
}