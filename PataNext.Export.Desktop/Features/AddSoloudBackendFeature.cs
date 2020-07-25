using System;
using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Audio;
using GameHost.Audio.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Simulation.Features.ShareWorldState;
using GameHost.Simulation.TabEcs;
using GameHost.Transports;
using GameHost.Worlds;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(AudioApplication))]
	public class AddSoloudBackendFeature : AppSystem
	{
		private DataBufferWriter header;
		
		private HeaderTransportDriver driver;
		private ThreadTransportDriver threadDriver;
		
		private IApplication application;

		public AddSoloudBackendFeature(WorldCollection collection) : base(collection)
		{
			header = new DataBufferWriter(0);
			
			AddDisposable(threadDriver = new ThreadTransportDriver(8));
			AddDisposable(driver = new HeaderTransportDriver(threadDriver));
			
			DependencyResolver.Add(() => ref application);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			threadDriver.Listen();
			
			driver.Header = header;
			
			World.Mgr.CreateEntity()
			     .Set<IFeature>(new SoLoudBackendFeature(driver));
			
			application.Global.Scheduler.Schedule(address => { application.AssignedEntity.Set(address); }, driver.TransportAddress, default);
		}

		public override void Dispose()
		{
			base.Dispose();
			header.Dispose();
		}
	}
}