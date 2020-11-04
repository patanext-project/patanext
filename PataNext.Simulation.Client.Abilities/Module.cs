using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;

[assembly: RegisterAvailableModule("Client Abilities", "guerro", typeof(PataNext.Simulation.Client.Abilities.Module))]

namespace PataNext.Simulation.Client.Abilities
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
					simulationApplication.Schedule(() => { simulationApplication.Data.Collection.GetOrCreate(typeof(TaterazayEnergyFieldClientProvider)); }, default);
				}
			}
		}
	}
}