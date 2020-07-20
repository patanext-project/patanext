using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation;

[assembly: RegisterAvailableModule("PataNext Simulation", "guerro", typeof(CustomModule))]

namespace PataNext.Module.Simulation
{
	public class CustomModule : GameHostModule
	{
		public CustomModule(Entity source, Context ctxParent, GameHostModuleDescription original) : base(source, ctxParent, original)
		{
			var logger = (ILogger) new DefaultAppObjectStrategy(this, new WorldCollection(ctxParent, null)).ResolveNow(typeof(ILogger));
			logger.Log(LogLevel.Information, "My custom module has been loaded!");
			
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (var listener in global.World.Get<IListener>())
			{
				Console.WriteLine(listener);
				if (listener is SimulationApplication simulationApplication)
				{
					//simulationApplication.Data.Collection.GetOrCreate(typeof(CreateEntitySystem));
				}
			}
		}
	}
}