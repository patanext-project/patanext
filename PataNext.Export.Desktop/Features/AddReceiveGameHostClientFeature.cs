using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Client;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Utility;
using Microsoft.Extensions.Logging;
using osu.Framework.Threading;
using PataNext.Export.Desktop.Visual;
using SharpInputSystem;
using SharpInputSystem.DirectX;
using ZLogger;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class AddReceiveGameHostClientFeature : AppSystem
	{
		private StartGameHostListener startGameHostListener;
		private ILogger               logger;

		private EntitySet launchSet;

		private IScheduler scheduler;
		private TaskScheduler taskScheduler;

		public AddReceiveGameHostClientFeature(WorldCollection collection) : base(collection)
		{
			collection.Mgr.CreateEntity()
			          .Set<IFeature>(new ReceiveGameHostClientFeature(0));
			DependencyResolver.Add(() => ref startGameHostListener);
			DependencyResolver.Add(() => ref logger);
			DependencyResolver.Add(() => ref scheduler);

			taskScheduler = new SameThreadTaskScheduler();

			launchSet = collection.Mgr.GetEntities()
			                      .With<LaunchClient>()
			                      .AsSet();
		}

		public override bool CanUpdate()
		{
			(taskScheduler as SameThreadTaskScheduler).Execute();
			
			if (_kb != null)
				_kb.Capture();
			
			return launchSet.Count > 0 && base.CanUpdate();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (startGameHostListener.Server.Value == null)
				return;
			
			var server     = startGameHostListener.Server.Value;
			var visualHwnd = World.Mgr.Get<VisualHWND>()[0].Value;
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
						UseShellExecute = false,
						RedirectStandardOutput = true
					}
				};
				process.Start();
				process.WaitForInputIdle();

				process.OutputDataReceived += (sender, eventArgs) =>
				{
					Console.WriteLine($"OUTPUT: {eventArgs.Data}");
				};

				var gameClient = new GameClient
				{
					ProcessId        = process.Id,
					IsHwndIntegrated = true // 
				};
				var clientEntity = World.Mgr.CreateEntity();
				clientEntity.Set(gameClient);

				TaskRunUtility.StartUnwrap(async cc =>
				{
					logger.ZLogInformation("Tryin' to get MainWindowHandle");
					while (!cc.IsCancellationRequested)
					{
						await Task.Delay(2500, cc);
						
						if (process.MainWindowHandle == IntPtr.Zero)
						{
							logger.ZLogInformation("Not yet found!");

							continue;
						}

						scheduler.Schedule(() =>
						{ 
							logger.ZLogInformation($"GameHandle (non resolved fully) = {process.MainWindowHandle}");

							var inputManager = SharpInputSystem.InputManager.CreateInputSystem(typeof(DirectXInputManagerFactory), new ParameterList
							{
								//new("WINDOW", process.MainWindowHandle)
								// what
								new("WINDOW", Process.GetCurrentProcess().MainWindowHandle)
							});
							logger.ZLogInformation($"mouse={inputManager.DeviceCount<SharpInputSystem.Mouse>()}");
							logger.ZLogInformation($"keyboard={inputManager.DeviceCount<SharpInputSystem.Keyboard>()}");

							(_kb = inputManager.CreateInputObject<Keyboard>(true, "")).EventListener = new KeyboardListener();

						}, default);
						break;
					}
				}, taskScheduler, CancellationToken.None).ContinueWith(t =>
				{
					logger.ZLogError($"Error when trying to attach to window! {t.Exception}");
				}, TaskContinuationOptions.OnlyOnFaulted);
			}

			launchSet.DisposeAllEntities();
		}

		private Keyboard _kb;

		class KeyboardListener : IKeyboardListener
		{
			public bool KeyPressed(KeyEventArgs  e)
			{
				//Console.WriteLine($"{e.Key} ({e.Text}) pressed!");
				return true;
			}

			public bool KeyReleased(KeyEventArgs e)
			{
				//Console.WriteLine($"{e.Key} ({e.Text}) released!");
				return true;
			}
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