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

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var result = await Service.GetPointer(entity.Get<GetAssetPointerRequest>().AssetGuid);
				entity.Set(new Response
				{
					ResPath = new(ResPath.EType.MasterServer, result.Author, result.Mod, result.Id)
				});
			}
		}
	}
}