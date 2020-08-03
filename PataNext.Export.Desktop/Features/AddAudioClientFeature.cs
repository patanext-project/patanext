using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Audio.Features;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Threading;
using GameHost.Simulation.Application;
using GameHost.Worlds;
using osu.Framework.Logging;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class AddAudioApplication : AppSystem
	{
		private IApplication application;
		private GlobalWorld  globalWorld;
		private IScheduler selfScheduler;

		private DataBufferWriter header;

		public AddAudioApplication(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref application);
			DependencyResolver.Add(() => ref globalWorld);
			DependencyResolver.Add(() => ref selfScheduler);
			
			AddDisposable(header = new DataBufferWriter(0));
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			globalWorld.Scheduler.Schedule(mainLoopUpdate, SchedulingParameters.AsOnce);
		}

		private HeaderTransportDriver driver;

		protected override void OnUpdate()
		{
			if (driver == null)
				return;
			
			
		}

		private void mainLoopUpdate()
		{
			if (driver != null)
				return;

			foreach (var entity in globalWorld.World)
			{
				if (!entity.TryGet(out TransportAddress addr) || !entity.Has<ConnectionToAudio>())
					continue;

				driver = new HeaderTransportDriver(addr.Connect()) {Header = header};
				break;
			}

			if (driver == null)
			{
				// continue scheduling until we have a valid driver
				globalWorld.Scheduler.Schedule(mainLoopUpdate, SchedulingParameters.AsOnce);
				return;
			}

			selfScheduler.Schedule(drv =>
			{
				var feature = new AudioClientFeature(drv, default);
				World.Mgr.CreateEntity()
				     .Set<IFeature>(feature);
				
				Logger.Log("Connected to audio backend", LoggingTarget.Runtime, LogLevel.Important);
			}, driver, default);
		}
	}
}