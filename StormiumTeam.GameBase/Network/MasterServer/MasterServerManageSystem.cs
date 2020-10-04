using System;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public class MasterServerManageSystem : AppSystemWithFeature<MasterServerFeature>
	{
		private ILogger logger;
		
		public MasterServerManageSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref logger);
		}

		protected override void OnFeatureAdded(MasterServerFeature obj)
		{
			// Maybe we should support more connections?
			if (Features.Count > 1)
				throw new InvalidOperationException("Only one MasterServer feature is allowed!");
		}

		protected override void OnFeatureRemoved(MasterServerFeature obj)
		{
			try
			{
				logger.ZLogInformation("Disconnecting from MasterServer...");
				obj.Channel.ShutdownAsync().Wait();
				logger.ZLogInformation("Disconnected from MasterServer.");
			}
			catch (Exception ex)
			{
				logger.ZLogError(ex, "Couldn't disconnect from MasterServer!");
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var feature in Features)
			{
				try
				{
					logger.ZLogInformation("Disconnecting from MasterServer...");
					feature.Channel.ShutdownAsync().Wait();
					logger.ZLogInformation("Disconnected from MasterServer.");
				}
				catch (Exception ex)
				{
					logger.ZLogError(ex, "Couldn't disconnect from MasterServer!");
				}
			}
		}
	}
}