using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.HostSerialization;
using PataNext.Module.RhythmEngine;
using PataponGameHost.Inputs;

namespace PataNext.Module.Simulation
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	public class SynchronizeComponent : AppSystem
	{
		private PresentationHostWorld.RestrictedHost restrictedHost;

		public SynchronizeComponent(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref restrictedHost);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			restrictedHost.Implementation.SubscribeComponent<RhythmEngineController>();
			restrictedHost.Implementation.SubscribeComponent<RhythmEngineOnNewBeat>();
			restrictedHost.Implementation.SubscribeComponent<PlayerInput>();
		}
	}
}