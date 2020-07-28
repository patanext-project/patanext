using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Cysharp.Text;
using GameHost.Core.Client;
using GameHost.Game;
using GameHost.IO;
using GameHost.Native;
using GameHost.Native.Char;
using GameHost.Native.Fixed;
using Microsoft.Extensions.Logging;
using Mono.Options;
using NuGet;
using PataNext.Export.Desktop.Updater;
using PataNext.Export.Desktop.Visual;
using Squirrel;
using ZLogger;

namespace PataNext.Export.Desktop
{
	class Program
	{
		static async Task Main(string[] args)
		{
			ClientBootstrap client        = null;
			
			var enableVisuals = false;
			var options = new OptionSet
			{
				{"c|client=", "name to the .json client bootstrap", c =>
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

						client = JsonSerializer.Deserialize<ClientBootstrap>(File.ReadAllText(c));
						var executable = client.ExecutablePath;
						if (!Path.IsPathRooted(executable))
							executable = $"{new FileInfo(c).Directory.FullName}/{executable}";
						client.ExecutablePath = executable;
					}
				},
				{"v|enable_visual", "enable visuals", v => enableVisuals = v != null},
				{"disable_visual", "enable visuals", v => enableVisuals = v == null}
			};
			options.Parse(args);

			if (client != null)
			{
				Console.WriteLine($"Client Path={client.ExecutablePath}, Args={client.LaunchArgs}");

				var process = new Process
				{
					StartInfo = new ProcessStartInfo(client.ExecutablePath, client.LaunchArgs)
				};
				process.Start();
			}

			if (enableVisuals)
			{
				using (osu.Framework.Platform.GameHost host = osu.Framework.Host.GetSuitableHost("PataNext.Visual"))
				using (osu.Framework.Game frameworkGame = new VisualGame())
				{
					host.Run(frameworkGame);
				}
			}
			else
			{
				runHost();
			}

			Environment.Exit(0);
		}
		
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		private static void runHost()
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

			var gameBootstrap = new GameBootstrap();
			gameBootstrap.GameEntity.Set(new GameName("PataNext"));
			gameBootstrap.GameEntity.Set(new GameUserStorage(new LocalStorage(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/PataNext")));
			gameBootstrap.GameEntity.Set(new GameLoggerFactory(loggerFactory));

			gameBootstrap.GameEntity.Set(typeof(GameHost.Inputs.DefaultActions.PressAction));
			gameBootstrap.GameEntity.Set(typeof(GameHost.Audio.UpdateSoLoudBackendDriverSystem));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Module.Simulation.CustomModule));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Simulation.Client.Module));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Feature.RhythmEngineAudio.CustomModule));
			gameBootstrap.GameEntity.Set(typeof(PataNext.Game.Module));

			gameBootstrap.Setup();
			while (gameBootstrap.Loop())
			{
			}
		}
	}
}