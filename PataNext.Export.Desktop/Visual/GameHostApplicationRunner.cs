using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Text;
using GameHost.Game;
using GameHost.IO;
using Microsoft.Extensions.Logging;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using ZLogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PataNext.Export.Desktop.Visual
{
	public class GameHostApplicationRunner : Drawable
	{
		private GameBootstrap gameBootstrap;
		
		protected override void LoadComplete()
		{
			base.LoadComplete();
			
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
				builder.AddZLoggerLogProcessor(new OsuLogProcessor());
			});

			gameBootstrap = new GameBootstrap();
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
		}

		protected override void Update()
		{
			base.Update();

			gameBootstrap.Loop();
		}

		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);
			
			if (gameBootstrap != null)
				gameBootstrap.Dispose();
		}

		public class OsuLogProcessor : IAsyncLogProcessor
		{
			private ZLoggerOptions options = new ZLoggerOptions { };

			private static readonly Logger logger = Logger.GetLogger("GameHost");

			public ValueTask DisposeAsync()
			{
				return default;
			}

			public void Post(IZLoggerEntry log)
			{
				try
				{
					logger.Add(log.FormatToString(options, null), castLevel(log.LogInfo.LogLevel), log.LogInfo.Exception);
				}
				finally
				{
					log.Return();
				}
			}

			private osu.Framework.Logging.LogLevel castLevel(LogLevel logLevel)
			{
				return logLevel switch
				{
					LogLevel.Trace => osu.Framework.Logging.LogLevel.Verbose,
					LogLevel.Debug => osu.Framework.Logging.LogLevel.Debug,
					LogLevel.Information => osu.Framework.Logging.LogLevel.Important,
					LogLevel.Warning => osu.Framework.Logging.LogLevel.Important,
					LogLevel.Error => osu.Framework.Logging.LogLevel.Error,
					LogLevel.Critical => osu.Framework.Logging.LogLevel.Error,
					LogLevel.None => osu.Framework.Logging.LogLevel.Verbose
				};
			}
		}
	}
}