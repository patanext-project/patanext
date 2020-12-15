using System.Collections.Generic;
using DiscordRPC;
using DiscordRPC.Logging;
using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using ZLogger;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(SimulationApplication))]
	[DontInjectSystemToWorld] // This will be added manually by AddApplicationSystem (we wouldn't want to have two apps)
	public class DiscordFeature : AppSystem
	{
		private DiscordRpcClient client;
		private ILogger          logger;

		public DiscordFeature(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref logger);
		}
		
		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			client = new DiscordRpcClient("609427243395055616", logger: new ConsoleLogger(DiscordRPC.Logging.LogLevel.Info));
			client.Initialize();

			client.SetPresence(new RichPresence
			{
				State = "In Menu",
				Assets = new()
				{
					LargeImageKey = "in-menu"
				}
			});
			
			AddDisposable(client);
		}

		~DiscordFeature()
		{
			if (!client.IsDisposed || client.IsInitialized)
				client.Deinitialize();
		}
	}
}