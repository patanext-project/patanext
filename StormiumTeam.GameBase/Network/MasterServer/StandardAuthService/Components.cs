using System;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using Microsoft.Extensions.Logging;
using STMasterServer.Shared.Services;
using STMasterServer.Shared.Services.Authentication;
using StormiumTeam.GameBase.Network.MasterServer.User;
using ZLogger;

namespace StormiumTeam.GameBase.Network.MasterServer.StandardAuthService
{
	public struct ConnectUserRequest
	{
		public enum EType
		{
			Login,
			Guid
		}

		public EType  Type;
		public string Data;
		public string HashedPassword;

		public static ConnectUserRequest ViaGuid(string guid, string password) =>
			new ConnectUserRequest
			{
				Type           = EType.Guid,
				Data           = guid,
				HashedPassword = StandardAuthUtility.CreateMD5(password)
			};

		public static ConnectUserRequest ViaLogin(string login, string password) =>
			new ConnectUserRequest
			{
				Type           = EType.Login,
				Data           = login,
				HashedPassword = StandardAuthUtility.CreateMD5(password)
			};
		
		public class Process : MasterServerRequestService<IStandardAuthService, ConnectUserRequest>
		{
			private CurrentUserSystem currentUserSystem;
			private ILogger           logger;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
				DependencyResolver.Add(() => ref logger);
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Console.WriteLine("yo0");
				var component = entity.Get<ConnectUserRequest>();

				ConnectResult result;
				switch (component.Type)
				{
					case EType.Login:
						result = await Service.ConnectViaLogin(component.Data, component.HashedPassword);
						break;
					case EType.Guid:
						result = await Service.ConnectViaGuid(component.Data, component.HashedPassword);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				Console.WriteLine("yo1");

				if (result.Token == null)
				{
					logger.ZLogCritical("Couldn't connect as '{0}' user (via {1} method)", component.Data, component.Type);
					return;
				}

				Console.WriteLine("yo3");
				currentUserSystem.Set(new UserToken(result.Guid, result.Token));
			}
		}
	}
}