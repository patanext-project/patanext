using System;
using System.Runtime;
using System.Threading.Tasks;
using Cysharp.Text;
using GameHost.Game;
using Microsoft.Extensions.Logging;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Logging;
using ZLogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PataNext.Export.Desktop.Visual
{
	public struct VisualHWND
	{
		public IntPtr   Value;
		public Vector2I Size;
		public bool     ShowIntegratedWindows;
		public bool     RequireSwap { get; set; }
	}
	
	public class GameHostApplicationRunner : Drawable
	{
		private GameBootstrap gameBootstrap;
		
		[BackgroundDependencyLoader]
		private void load(GameBootstrap gameBootstrap)
		{
			this.gameBootstrap = gameBootstrap;
			
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
			
			gameBootstrap.GameEntity.Set(new GameLoggerFactory(loggerFactory));
			gameBootstrap.Setup();
		}

		protected override void Update()
		{
			base.Update();
			
			gameBootstrap.Loop();
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
					logger.Add($"[{log.LogInfo.CategoryName}] {log.FormatToString(options, null)}", castLevel(log.LogInfo.LogLevel), log.LogInfo.Exception);
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