using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.MasterServerShared.Services;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.User;

namespace PataNext.Module.Simulation.Network.MasterServer.Services
{
	public struct CreateGameSaveRequest : IComponentData
	{
		public string Name;

		public CreateGameSaveRequest(string name)
		{
			Name = name;
		}

		public struct Response : IComponentData
		{
			public string SaveId;
		}

		public class Process : MasterServerRequestService<IGameSaveService, CreateGameSaveRequest>
		{
			private CurrentUserSystem currentUserSystem;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var representation = await Service.CreateSave(currentUserSystem.Token, entity.Get<CreateGameSaveRequest>().Name);
				entity.Set(new Response {SaveId = representation});
			}
		}
	}

	public struct ListGameSaveRequest
	{
		public struct Response
		{
			public string[] Results;
		}

		public class Process : MasterServerRequestService<IGameSaveService, ListGameSaveRequest>
		{
			private CurrentUserSystem currentUserSystem;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var saveIds = await Service.ListSaves(currentUserSystem.Token.Representation);
				entity.Set(new Response {Results = saveIds});
			}
		}
	}
}