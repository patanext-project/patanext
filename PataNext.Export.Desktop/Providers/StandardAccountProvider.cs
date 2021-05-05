using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using osu.Framework.Bindables;
using PataNext.Export.Desktop.Visual;
using PataNext.Export.Desktop.Visual.Dependencies;
using StormiumTeam.GameBase.Network.MasterServer.StandardAuthService;
using StormiumTeam.GameBase.Network.MasterServer.User;
using StormiumTeam.GameBase.Network.MasterServer.UserService;
using StormiumTeam.GameBase.Network.MasterServer.Utility;

namespace PataNext.Export.Desktop.Providers
{
	public class StandardAccountProvider : IAccountProvider, IHasDiscordAccountSupport
	{
		private readonly INotificationsProvider notifications;
		
		public StandardAccountProvider(INotificationsProvider notifications)
		{
			this.notifications = notifications;
		}
		
		public Bindable<LauncherAccount> Current { get; } = new();

		public void ConnectTraditional(string login, string password)
		{
			simulationScheduler.Schedule(args =>
			{
				args.CreateEntity().Set(ConnectUserRequest.ViaLogin(login, password));
			}, simulationWc.Mgr, SchedulingParametersWithArgs.AsOnce);
			
			Console.WriteLine("1.");
			simulationScheduler.Schedule(args =>
			{
				Console.WriteLine("2.");
				/*RequestUtility.New(simulationWc.Mgr, ConnectUserRequest.ViaLogin(args.login, args.password))
				              .GetAsync<ConnectUserRequest.Error>()
				              .ContinueWithScheduler(uiScheduler, error =>
				              {
					              Current.Value = new()
					              {
						              Error   = true,
						              Message = error.Message
					              };
					              
					              notifications.Push(new()
					              {
						              Title = "Connection Error!",
						              Text = error.Message,
						              Action = () => { }
					              });
				              });*/
			}, (simulationWc.Mgr, login, password), SchedulingParametersWithArgs.AsOnce);
		}

		public void Disconnect()
		{
			simulationScheduler.Schedule(args => { args.CreateEntity().Set(new DisconnectUserRequest()); }, simulationWc.Mgr, SchedulingParametersWithArgs.AsOnce);
		}

		public void ConnectDiscord()
		{
			throw new NotImplementedException("not yet implemented");
		}

		private WorldCollection simulationWc;
		private IScheduler      simulationScheduler;

		private IScheduler uiScheduler;
		
		internal void SetBackend(IScheduler uiScheduler, WorldCollection simulationWc)
		{
			this.uiScheduler = uiScheduler;
			
			this.simulationWc        = simulationWc;
			this.simulationScheduler = new ContextBindingStrategy(simulationWc.Ctx, true).Resolve<IScheduler>();

			simulationWc.Mgr.SubscribeComponentChanged((in Entity _, in CurrentUser _, in CurrentUser curr) =>
			{
				if (curr.Value.Token is null)
				{
					uiScheduler.Schedule(() => {Current.SetDefault();}, default);
					return;
				}

				RequestUtility.New(simulationWc.Mgr, new GetUserLoginRequest(curr.Value.Representation))
				              .GetAsync<GetUserLoginRequest.Result>()
				              .ContinueWithScheduler(uiScheduler, result =>
				              {
					              Current.Value = new LauncherAccount
					              {
						              IsConnected = true,
						              Nickname = result.Login
					              };
				              });
			});
		}
	}
}