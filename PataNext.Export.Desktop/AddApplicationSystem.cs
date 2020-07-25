using System;
using System.Collections.Generic;
using System.Threading;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Audio.Applications;
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
			AddApp("Simulation", new SimulationApplication(globalWorld, null));
			AddApp("Audio", new AudioApplication(globalWorld, null));
		}

		private Entity AddApp<T>(string name, T app)
			where T : class, IApplication, IListener
		{
			var listener = World.Mgr.CreateEntity();
			listener.Set<ListenerCollectionBase>(new ThreadListenerCollection(name, ccs.Token));

			var systems = new List<Type>();
			AppSystemResolver.ResolveFor<T>(systems);

			foreach (var type in systems)
				app.Data.Collection.GetOrCreate(type);

			var applicationEntity = World.Mgr.CreateEntity();
			applicationEntity.Set<IListener>(app);
			applicationEntity.Set(new PushToListenerCollection(listener));
			return applicationEntity;
		}

		public override void Dispose()
		{
			base.Dispose();
			ccs.Cancel();
		}
	}
}