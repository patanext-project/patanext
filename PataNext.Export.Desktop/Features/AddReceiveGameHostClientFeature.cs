using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Net.NetworkInformation;
using System.Threading;
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

		public AddReceiveGameHostClientFeature(WorldCollection collection) : base(collection)
		{
			collection.Mgr.CreateEntity()
			          .Set<IFeature>(new ReceiveGameHostClientFeature(0));
			DependencyResolver.Add(() => ref startGameHostListener);
			DependencyResolver.Add(() => ref logger);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			var clientBootstrapSpan = World.Mgr.Get<ClientBootstrap>();
			if (clientBootstrapSpan.Length == 0)
				return;

			startGameHostListener.Server.Subscribe((_, server) =>
			{
				if (server == null)
					return;
				
				var visualHwnd = World.Mgr.Get<VisualHWND>().FirstOrDefault().Value;
				foreach (var client in World.Mgr.Get<ClientBootstrap>())
				{
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
							Arguments      = args,
							FileName       = client.ExecutablePath,
							CreateNoWindow = true,
							UseShellExecute = true
						}
					};
					process.Start();
					process.WaitForInputIdle();

					var gameClient = new GameClient
					{
						ProcessId        = process.Id,
						IsHwndIntegrated = true
					};
					var clientEntity = World.Mgr.CreateEntity();
					clientEntity.Set(gameClient);
				}

			}, true);
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
				catch (ArgumentException ex)
				{

				}
			}
		}
	}
}