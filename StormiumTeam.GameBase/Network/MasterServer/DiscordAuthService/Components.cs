using System;
using System.Threading.Tasks;
using Cysharp.Text;
using DefaultEcs;
using GameHost.Core.Ecs;
using Microsoft.Extensions.Logging;
using STMasterServer.Shared.Services;
using STMasterServer.Shared.Services.Authentication;
using StormiumTeam.GameBase.Network.MasterServer.User;
using ZLogger;

namespace StormiumTeam.GameBase.Network.MasterServer.StandardAuthService
{
	public struct ConnectUserWithDiscordRequest
	{
		public ulong                                UserId;
		public Func<DiscordAuthBegin, Task<string>> LobbyStep;

		public struct Error
		{
			public string Message;
		}

		public static ConnectUserWithDiscordRequest Create(ulong id, Func<DiscordAuthBegin, Task<string>> lobbyStep) =>
			new()
			{
				UserId    = id,
				LobbyStep = lobbyStep
			};

		public class Process : MasterServerRequestService<IDiscordAuthService, ConnectUserWithDiscordRequest>
		{
			private CurrentUserSystem currentUserSystem;
			private ILogger           logger;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
				DependencyResolver.Add(() => ref logger);
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Console.WriteLine("yo0");
				var component = entity.Get<ConnectUserWithDiscordRequest>();

				ConnectResult result = default;
				try
				{
					var step    = await Service.BeginAuth(component.UserId);
					var lobbyId = await component.LobbyStep(step);

					result = await Service.FinalizeAuth(lobbyId, step.StepToken);
				}
				catch (Exception ex)
				{
					logger.ZLogError(ex, "");
				}

				Console.WriteLine("yo1");

				if (result.Token == null)
				{
					logger.ZLogCritical("Couldn't connect with Discord Id {0}", component.UserId);
					return e => { e.Set(new Error {Message = ZString.Format("Couldn't connect with Discord Id {0}", component.UserId)}); };
				}

				Console.WriteLine("yo3");
				return e => { currentUserSystem.Set(new(result.Guid, result.Token)); };
			}
		}
	}
}