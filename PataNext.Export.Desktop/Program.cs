using System;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Text;
using GameHost.Core.Client;
using GameHost.Game;
using GameHost.IO;
using Microsoft.Extensions.Logging;
using Mono.Options;
using ZLogger;

namespace PataNext.Export.Desktop
{
	class Program
	{
		static async Task Main(string[] args)
		{
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

			var enableVisuals    = true;
			var launchClientJson = string.Empty;
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
				{"disable_visual", "enable visuals", v => enableVisuals = v == null}
			};
			options.Parse(args);
			
			var clientDirectory = new DirectoryInfo(Environment.CurrentDirectory + "/clients/");
			if (!clientDirectory.Exists)
			{
				clientDirectory.Create();
			}

			var gameBootstrap = new GameBootstrap();
			gameBootstrap.GameEntity.Set(new GameName("PataNext"));
			gameBootstrap.GameEntity.Set(new GameUserStorage(new LocalStorage(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/PataNext")));

			gameBootstrap.GameEntity.Set(typeof(GameHost.Inputs.Systems.SendPushedInputLayoutsToBackend));
			gameBootstrap.GameEntity.Set(typeof(GameHost.Audio.UpdateSoLoudBackendDriverSystem));
			gameBootstrap.GameEntity.Set(typeof(StormiumTeam.GameBase.Module));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Module.Simulation.CustomModule));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Simulation.Client.Module));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Simulation.Mixed.Abilities.Module));
			gameBootstrap.GameEntity.Set(typeof(Feature.RhythmEngineAudio.CustomModule));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Game.Module));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Game.Client.Resources.Module));

			foreach (var clientData in clientDirectory.GetFiles("*.json", SearchOption.TopDirectoryOnly))
			{
				var client     = JsonSerializer.Deserialize<ClientBootstrap>(File.ReadAllText(clientData.FullName));
				var executable = client.ExecutablePath;
				if (!Path.IsPathRooted(executable))
					executable = $"{clientData.Directory.FullName}/{executable}";
				client.ExecutablePath = executable;

				var clientBootstrap = gameBootstrap.Global.World.CreateEntity();
				clientBootstrap.Set(client);

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
			while (gameBootstrap.Loop())
			{
				Thread.Sleep(10);
			}
			gameBootstrap.Dispose();
		}
	}
}