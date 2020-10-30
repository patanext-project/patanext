using System;
using System.Diagnostics;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Client;
using GameHost.Core.Ecs;
using Microsoft.Extensions.Logging;
using NetFabric.Hyperlinq;
using PataNext.Export.Desktop.Visual;
using ZLogger;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class AddReceiveGameHostClientFeature : AppSystem
	{
		private StartGameHostListener startGameHostListener;
		private ILogger               logger;

		private EntitySet launchSet;

		public AddReceiveGameHostClientFeature(WorldCollection collection) : base(collection)
		{
			collection.Mgr.CreateEntity()
			          .Set<IFeature>(new ReceiveGameHostClientFeature(0));
			DependencyResolver.Add(() => ref startGameHostListener);
			DependencyResolver.Add(() => ref logger);

			launchSet = collection.Mgr.GetEntities()
			                      .With<LaunchClient>()
			                      .AsSet();
		}

		public override bool CanUpdate()
		{
			return launchSet.Count > 0 && base.CanUpdate();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (startGameHostListener.Server.Value == null)
				return;

			var server     = startGameHostListener.Server.Value;
			var visualHwnd = World.Mgr.Get<VisualHWND>().FirstOrDefault().Value;
			foreach (var ent in launchSet.GetEntities())
			{
				var launchBootstrapEntity = ent.Get<LaunchClient>().entity;
				Debug.Assert(launchBootstrapEntity != null, "clientEntity != null");
				Debug.Assert(launchBootstrapEntity.Has<ClientBootstrap>(), "clientEntity.Has<ClientBootstrap>()");

				var client = launchBootstrapEntity.Get<ClientBootstrap>();

				var args = client.LaunchArgs;
				args = args.Replace("{GameHostPort}", server.LocalPort.ToString());
				args = args.Replace("{GameHostProcessId}", Process.GetCurrentProcess().Id.ToString());
				if (visualHwnd != IntPtr.Zero)
				{
					args = args.Replace("{VisualHWND}", visualHwnd.ToInt32().ToString());
				}

				logger.ZLogInformation("Client launched from '{0}' with args: '{1}'", client.ExecutablePath, args);

				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						Arguments       = args,
						FileName        = client.ExecutablePath,
						CreateNoWindow  = true,
						UseShellExecute = true
					}
				};
				process.Start();
				process.WaitForInputIdle();

				var gameClient = new GameClient
				{
					ProcessId        = process.Id,
					IsHwndIntegrated = false // 
				};
				var clientEntity = World.Mgr.CreateEntity();
				clientEntity.Set(gameClient);
			}

			launchSet.DisposeAllEntities();
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach (var gameClient in World.Mgr.Get<GameClient>())
			{
				try
				{
					var gameClientProcess = gameClient.Process;
					if (!gameClientProcess.HasExited)
						gameClientProcess.Kill();
				}
				catch (ArgumentException)
				{

				}
			}
		}
	}
}