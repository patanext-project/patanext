using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase;

[assembly: RegisterAvailableModule("PataNext Standard Abilities", "guerro", typeof(PataNext.Simulation.Mixed.Abilities.Module))]

namespace PataNext.Simulation.Mixed.Abilities
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultMarchAbilityProvider));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultMarchAbilitySystem));
					
					simulationApplication.Data.Collection.GetOrCreate(typeof(Subset.DefaultSubsetMarchAbilitySystem));
				}
			}
		}
	}
}