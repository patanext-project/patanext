using System;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using STMasterServer.Shared.Services;
using StormiumTeam.GameBase.Network.MasterServer.User;

namespace StormiumTeam.GameBase.Network.MasterServer.UserService
{
	public struct GetUserLoginRequest
	{
		public string Representation;

		public GetUserLoginRequest(string representation)
		{
			Representation = representation;
		}

		public struct Result
		{
			public string Login;
		}

		public class Process : MasterServerRequestService<IConnectionService, GetUserLoginRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}
			
			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var login = await Service.GetLogin(entity.Get<GetUserLoginRequest>().Representation);
				return ent => ent.Set(new Result {Login = login});
			}
		}
	}

	public struct DisconnectUserRequest
	{
		public string Token;

		public DisconnectUserRequest(string token)
		{
			Token = token;
		}

		public class Process : MasterServerRequestService<IConnectionService, DisconnectUserRequest>
		{
			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
			}

			private readonly Action<Entity> none = _ => { };

			private CurrentUserSystem currentUserSystem;

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				await Service.Disconnect(entity.Get<DisconnectUserRequest>().Token);

				currentUserSystem.Unset();
				
				return none;
			}
		}
	}
}