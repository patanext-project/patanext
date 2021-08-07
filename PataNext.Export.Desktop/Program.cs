using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Text;
using GameHost.Core.Client;
using GameHost.Game;
using GameHost.Injection;
using GameHost.IO;
using Microsoft.Extensions.Logging;
using Mono.Options;
using osu.Framework.Threading;
using PataNext.Export.Desktop.Bootstrap;
using StormiumTeam.GameBase.Bootstrap;
using ZLogger;

namespace PataNext.Export.Desktop
{
	class Program
	{
		private static int currentVal;
		
		
		[DllImport("kernel32.dll")]
		static extern bool AttachConsole(int dwProcessId);
		
		static async Task Main(string[] args)
		{
			if (args.Contains("rider"))
			{
				// For some weird reasons, nothing is getting written to the console if we don't put that line?
				AttachConsole(-1);
			}

			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

			var enableVisuals    = false;
			var launchClientJson = string.Empty;
			var targetBootstrap  = ConfigurableBootstrap.NameId;
			var options = new OptionSet
			{
				{
					"c|client=", "name to the .json client bootstrap", c =>
					{
						if (c == null)
							return;

						if (Path.IsPathRooted(c))
						{
							c += ".json";
						}
						else
						{
							c = $"clients/{c}.json";
						}

						launchClientJson = c;
					}
				},
				{"v|enable_visual", "enable visuals", v => enableVisuals = v != null},
				{"disable_visual", "enable visuals", v => enableVisuals = v == null},
				{"b|bootstrap", "set bootstrap", b => targetBootstrap = b ?? ConfigurableBootstrap.NameId}
			};
			options.Parse(args);
			
			var clientDirectory = new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/clients/");
			if (!clientDirectory.Exists)
			{
				clientDirectory.Create();
			}

			var gameBootstrap = new GameBootstrap();

			gameBootstrap.Global.Collection.GetOrCreate(wc => new BootstrapManager(wc));
			
			gameBootstrap.GameEntity.Set(new GameName("PataNext"));
			gameBootstrap.GameEntity.Set(new GameUserStorage(new LocalStorage(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/PataNext")));

			gameBootstrap.GameEntity.Set(typeof(GameHost.Inputs.Systems.SharpDxInputSystem));
			gameBootstrap.GameEntity.Set(typeof(GameHost.Audio.UpdateSoLoudBackendDriverSystem));
			gameBootstrap.GameEntity.Set(typeof(StormiumTeam.GameBase.Module));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Module.Simulation.CustomModule));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Simulation.Client.Module));
			gameBootstrap.GameEntity.Set(typeof(CoreAbilities.Mixed.Module));
			gameBootstrap.GameEntity.Set(typeof(CoreMissions.Mixed.Module));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Simulation.Client.Abilities.Module));
			gameBootstrap.GameEntity.Set(typeof(Feature.RhythmEngineAudio.CustomModule));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Game.Module));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Game.Client.Resources.Module));
			
			gameBootstrap.GameEntity.Set(new TargetBootstrap(targetBootstrap));

			Console.WriteLine("getting clients at " + clientDirectory.FullName);
			foreach (var clientData in clientDirectory.GetFiles("*.json", SearchOption.TopDirectoryOnly))
			{
				var client     = JsonSerializer.Deserialize<ClientBootstrap>(File.ReadAllText(clientData.FullName));
				var executable = client.ExecutablePath;
				if (!Path.IsPathRooted(executable))
					executable = $"{clientData.Directory.FullName}/{executable}";
				client.ExecutablePath = executable;

				var clientBootstrap = gameBootstrap.Global.World.CreateEntity();
				clientBootstrap.Set(client);

				Console.WriteLine($"found {client.ExecutablePath} ({clientData.Name})");
				if (launchClientJson != string.Empty && clientData.FullName == new FileInfo(launchClientJson).FullName)
				{
					gameBootstrap.Global.World.CreateEntity().Set(new LaunchClient(clientBootstrap));
				}
			}

			if (enableVisuals)
			{
				using (osu.Framework.Platform.GameHost host = osu.Framework.Host.GetSuitableHost("PataNext.Visual"))
				{
					host.Run(new VisualGame(gameBootstrap));
				}
			}
			else
			{
				runHost(gameBootstrap);
			}

			Environment.Exit(0);
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		private static void runHost(GameBootstrap gameBootstrap)
		{
			AllocConsole();

			GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

			var loggerFactory = LoggerFactory.Create(builder =>
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

			gameBootstrap.GameEntity.Set(new GameLoggerFactory(loggerFactory));
			gameBootstrap.Setup();

			var dependencyResolver = new DependencyResolver(gameBootstrap.Global.Scheduler, gameBootstrap.Global.Context, "Program")
			{
				DefaultStrategy = new ResolveSystemStrategy(gameBootstrap.Global.Collection)
			};
			dependencyResolver.Add<BootstrapManager>();
			dependencyResolver.OnComplete(deps =>
			{
				gameBootstrap.Global.Scheduler.Schedule(() =>
				{
					var array = deps.ToArray();
					(array[0] as BootstrapManager).ExecuteArgument(gameBootstrap.GameEntity.Get<TargetBootstrap>().NameId);
					
				}, default);
			});

			while (gameBootstrap.Loop())
			{
				Thread.Sleep(10);
			}
			gameBootstrap.Dispose();
		}
	}
}