using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Utility;
using GameHost.Worlds;

[assembly: RegisterAvailableModule("PN Core Missions 'Server'", "guerro", typeof(PataNext.CoreMissions.Server.Module))]

namespace PataNext.CoreMissions.Server
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{		
			var systems = new PooledList<Type>();
			AddDisposable(systems);
			
			AppSystemResolver.ResolveFor<SimulationApplication>(GetType().Assembly, systems);
			
			AddDisposable(ApplicationTracker.Track(this, (SimulationApplication simulationApplication) =>
			{
				foreach (var type in systems)
					AddDisposable((IDisposable)simulationApplication.Data.Collection.GetOrCreate(type));
			}));
		}
	}
}