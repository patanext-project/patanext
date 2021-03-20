using System;
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
				Console.WriteLine($"asking for {entity.Get<GetItemAssetPointerRequest>().ItemGuid}");
				var result = await Service.GetAssetPointer(entity.Get<GetItemAssetPointerRequest>().ItemGuid);
				return e => e.Set(new Response
				{
					ResPath = new(ResPath.EType.MasterServer, result.Author, result.Mod, result.Id)
				});
			}
		}
	}
}