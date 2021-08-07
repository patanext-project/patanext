using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Core.Modules.Feature;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Utility;
using GameHost.Worlds;
using PataNext.Game;
using StormiumTeam.GameBase;

[assembly: RegisterAvailableModule("PN Core Missions 'Mixed'", "guerro", typeof(PataNext.CoreMissions.Mixed.Module))]

namespace PataNext.CoreMissions.Mixed
{
	public class Module : GameHostModule
	{
		private List<Type> systems = new();

		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			AppSystemResolver.ResolveFor<SimulationApplication>(GetType().Assembly, systems);

			AddDisposable(ApplicationTracker.Track(this, (SimulationApplication simulationApplication) =>
			{
				foreach (var type in systems)
					AddDisposable((IDisposable)simulationApplication.Data.Collection.GetOrCreate(type));
			}));
			
			global.Scheduler.Schedule(tryLoadModule, SchedulingParameters.AsOnce);
		}
		
		private void tryLoadModule()
		{
			var global = new ContextBindingStrategy(Ctx.Parent, true).Resolve<GlobalWorld>();
			foreach (var ent in global.World)
			{
				if (ent.TryGet(out RegisteredModule registeredModule)
				    && registeredModule.State == ModuleState.None
				    && (registeredModule.Description.NameId == "PataNext.CoreMissions.Server"))
				{
					Console.WriteLine("[Missions] Load Server Module!");
					global.World.CreateEntity()
					      .Set(new RequestLoadModule {Module = ent});
					return;
				}
			}
			
			global.Scheduler.Schedule(tryLoadModule, SchedulingParameters.AsOnce);
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			systems.Clear();
		}
	}
}