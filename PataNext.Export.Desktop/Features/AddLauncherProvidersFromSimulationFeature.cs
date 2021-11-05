using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Worlds;
using PataNext.Export.Desktop.Providers;
using PataNext.Export.Desktop.Visual.Dependencies;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class AddLauncherProvidersFromSimulationFeature : AppSystem
	{
		private GlobalWorld             globalWorld;

		public AddLauncherProvidersFromSimulationFeature(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref globalWorld);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			var accountProvider = new ContextBindingStrategy(Context, true).Resolve<StandardAccountProvider>();
			accountProvider?.SetBackend(globalWorld.Scheduler, World);
		}
	}
}