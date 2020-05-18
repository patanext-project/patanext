using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.HostSerialization;
using PataNext.Module.Simulation.RhythmEngine;

namespace PataNext.Module.Simulation
{
	public class SynchronizeComponent : AppSystem
	{
		private PresentationHostWorld.RestrictedHost restrictedHost;
		
		public SynchronizeComponent(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref restrictedHost);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			restrictedHost.Implementation.SubscribeComponent<RhythmEngineOnNewBeat>();
		}
	}
}