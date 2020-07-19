using System.Collections.Generic;
using System.Threading;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Execution;
using GameHost.Threading;
using GameHost.Worlds;
using PataponGameHost;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class AddApplicationSystem : AppSystem
	{
		private GlobalWorld             globalWorld;
		private CancellationTokenSource ccs;

		public AddApplicationSystem(WorldCollection collection) : base(collection)
		{
			ccs = new CancellationTokenSource();

			DependencyResolver.Add(() => ref globalWorld);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			var listener = World.Mgr.CreateEntity();
			listener.Set<ListenerCollectionBase>(new ThreadListenerCollection("Simulation", ccs.Token));

			var simulationAppEntity = World.Mgr.CreateEntity();
			simulationAppEntity.Set(new SimulationApplication(globalWorld, null));
			simulationAppEntity.Set(new PushToListenerCollection(listener));
		}

		public override void Dispose()
		{
			base.Dispose();
			ccs.Cancel();
		}
	}
}