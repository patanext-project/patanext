using System;
using System.Collections.Generic;
using System.Threading;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Execution;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;

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

			var systems = new List<Type>();
			AppSystemResolver.ResolveFor<SimulationApplication>(systems);

			var simulationApp = new SimulationApplication(globalWorld, null);
			foreach (var type in systems)
				simulationApp.Data.Collection.GetOrCreate(type);

			var simulationAppEntity = World.Mgr.CreateEntity();
			simulationAppEntity.Set<IListener>(simulationApp);
			simulationAppEntity.Set(new PushToListenerCollection(listener));
		}

		public override void Dispose()
		{
			base.Dispose();
			ccs.Cancel();
		}
	}
}