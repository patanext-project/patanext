using System;
using System.Runtime;
using Cysharp.Text;
using GameHost.Game;
using GameHost.IO;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace PataNext.Export.Desktop
{
	class Program
	{
		static void Main(string[] args)
		{
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

			using var game = new GameBootstrap();
			game.GameEntity.Set(new GameName("PataNext"));
			game.GameEntity.Set(new GameUserStorage(new LocalStorage(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/PataNext")));
			game.GameEntity.Set(new GameLoggerFactory(loggerFactory));
			
			game.GameEntity.Set(typeof(GameHost.Inputs.DefaultActions.PressAction));
			game.GameEntity.Set(typeof(PataNext.Module.Simulation.CustomModule));
			game.GameEntity.Set(typeof(PataNext.Simulation.Client.Module));

			game.Run();
		}
	}
}