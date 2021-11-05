using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Audio;
using GameHost.Audio.Applications;
using GameHost.Core.Ecs;
using GameHost.Transports;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(AudioApplication))]
	public class AddSoloudBackendFeature : AppSystem
	{
		private DataBufferWriter header;
		
		private HeaderTransportDriver driver;
		private ThreadTransportDriver innerDriver;
		
		private IApplication application;

		public AddSoloudBackendFeature(WorldCollection collection) : base(collection)
		{
			header = new DataBufferWriter(0);
			
			AddDisposable(innerDriver = new ThreadTransportDriver(8));
			AddDisposable(driver      = new HeaderTransportDriver(innerDriver));
			
			DependencyResolver.Add(() => ref application);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			innerDriver.Listen();
			
			driver.Header = header;
			
			World.Mgr.CreateEntity()
			     .Set<IFeature>(new SoLoudBackendFeature(driver));
			
			application.Global.Scheduler.Schedule(address =>
			{
				application.AssignedEntity.Set(address);
				application.AssignedEntity.Set<ConnectionToAudio>();
			}, driver.TransportAddress, default);
		}

		public override void Dispose()
		{
			base.Dispose();
			header.Dispose();
		}
	}
}