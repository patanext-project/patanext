using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Text;
using DryIoc;
using GameHost;
using GameHost.Applications;
using GameHost.Core.Game;
using GameHost.Core.Logging;
using GameHost.Injection;
using GameHost.Input.OpenTKBackend;
using Microsoft.Extensions.Logging;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using ZLogger;

namespace PataponGameHost
{
	public class ClientGameBootstrap : GameBootstrapBase
	{
		/// <summary>
		/// Kinda ugly, but it give us the possibility to make sure internal assemblies override external assemblies...
		/// </summary>
		public List<Assembly> OverrideExternalAssemblies = new List<Assembly>();

		private GameWindow window;
		private Instance   instance;

		private ILoggerFactory             loggerFactory;
		private ILogger appLogger;

		protected override void RunGame()
		{
			var disposableObjects = new List<IDisposable>();
			disposableObjects.Add(loggerFactory);

			window = new GameFrame(Context, disposableObjects);

			var logPublisher = new LogPublisher();
			loggerFactory.AddProvider(logPublisher);

			Context.Bind<Instance, Instance>(Instance.CreateInstance<Instance>("Client", Context));
			Context.Bind<INativeWindow, GameWindow>(window);
			Context.Bind<IGameWindow, GameWindow>(window);
			Context.Bind<ILogger>(appLogger);
			Context.Bind(logPublisher);
			Context.Bind(loggerFactory);
			
			appLogger.ZLog(LogLevel.Information, "GAME STARTED!");

			GameAppShared.Init(Context, ref disposableObjects);

			var inputClient = new GameInputThreadingClient();
			inputClient.Connect();

			// Set backends (later there should be support for Unity inputs)
			inputClient.SetBackend<OpenTkInputBackend>();

			var simulationClient = new GameSimulationThreadingClient();
			simulationClient.Connect();

			simulationClient.AddInstance(Context.Container.Resolve<Instance>());

			var mainThreadClient = new MainThreadClient();

			window.UpdateFrame += args => { mainThreadClient.Listener.Update(); };

			window.Run();

			foreach (var obj in disposableObjects)
				obj.Dispose();

			Dispose();
		}

		public override bool IsRunning => window.Exists && !window.IsExiting;

		public override GameInformation GetGameInformation()
		{
			return new GameInformation
			{
				Name         = "PataNext",
				NameAsFolder = "PataNextClient"
			};
		}

		public ClientGameBootstrap(Context context) : base(context)
		{
			loggerFactory = LoggerFactory.Create(builder =>
			{
				static void opt(ZLoggerOptions options)
				{
					var prefixFormat = ZString.PrepareUtf8<LogLevel, DateTime, string>("[{0}, {1}, {2}] ");
					options.PrefixFormatter = (writer, info) => prefixFormat.FormatTo(ref writer, info.LogLevel, info.Timestamp.DateTime.ToLocalTime(), info.CategoryName);
				}

				builder.ClearProviders();
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddZLoggerRollingFile((offset, i) => $"logs/{offset.ToLocalTime():yyyy-MM-dd}_{i:000}.log",
					x => x.ToLocalTime().Date,
					8196,
					opt);
				builder.AddZLoggerFile("log.json");


				builder.AddZLoggerConsole((Action<ZLoggerOptions>) opt);
			});

			appLogger = loggerFactory.CreateLogger("GameBootstrap");

			AppDomain.CurrentDomain.UnhandledException += (sender, args) => { OnException((Exception) args.ExceptionObject); };
			TaskScheduler.UnobservedTaskException      += (sender, args) => { OnException((Exception) args.Exception); };
		}

		private void OnException(Exception ex)
		{
			appLogger.ZLogError(ex, "Unhandled Exception.");

			// quite game
			window.Close();
		}
	}
}