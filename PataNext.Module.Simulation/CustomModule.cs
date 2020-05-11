using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Modding;
using GameHost.Entities;
using GameHost.Injection;
using PataNext.Module.Simulation;
using PataNext.Module.Simulation.RhythmEngine;
using PataNext.Module.Simulation.RhythmEngine.Data;

[assembly: ModuleDescription("PataNext Simulation", "guerro", typeof(CustomModule))]

namespace PataNext.Module.Simulation
{
	public class CustomModule : CModule
	{
		public CustomModule(Entity source, Context ctxParent, SModuleInfo original) : base(source, ctxParent, original)
		{
			Console.WriteLine("My custom module has been loaded!");

			var simulationClient = new GameSimulationThreadingClient();
			simulationClient.Connect();

			simulationClient.InjectAssembly(GetType().Assembly);

			var inputClient = new GameInputThreadingClient();
			inputClient.Connect();

			inputClient.InjectAssembly(GetType().Assembly);
		}
	}

	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	public class TestModSystem : AppSystem
	{
		private IManagedWorldTime worldTime;

		public TestModSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			var ent = World.Mgr.CreateEntity();
			ent.Set(new RhythmEngineController {State      = RhythmEngineState.Playing, StartTime = worldTime.Total.Add(TimeSpan.FromSeconds(2))});
			ent.Set(new RhythmEngineSettings {BeatInterval = TimeSpan.FromSeconds(0.5), MaxBeat   = 4});
			ent.Set(new RhythmEngineLocalState());
			ent.Set(new RhythmEngineLocalCommandBuffer());
		}
	}
}