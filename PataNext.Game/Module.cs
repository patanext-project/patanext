using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Game;

[assembly: RegisterAvailableModule("PataNext.Game", "guerro", typeof(Module))]

namespace PataNext.Game
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global  = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			var systems = new List<Type>();
			AppSystemResolver.ResolveFor<SimulationApplication>(GetType().Assembly, systems);

			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					foreach (var type in systems)
						simulationApplication.Data.Collection.GetOrCreate(type);
				}
			}
		}
	}
}