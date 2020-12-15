using System;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Client.Systems;
using PataNext.Feature.RhythmEngineAudio.BGM.Directors;
using PataNext.Simulation.Client;
using PataNext.Simulation.Client.Systems;

namespace PataNext.Feature.RhythmEngineAudio
{
	public class CustomModule : GameHostModule
	{
		public CustomModule(Entity source, Context ctxParent, GameHostModuleDescription original) : base(source, ctxParent, original)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (var listener in global.World.Get<IListener>())
				if (listener is SimulationApplication simulationApplication)
				{
					if (!simulationApplication.AssignedEntity.Has<IClientSimulationApplication>())
						continue;

					Console.WriteLine("Add to " + simulationApplication.AssignedEntity.Get<ApplicationName>().Value);
					simulationApplication.Schedule(() =>
					{
						simulationApplication.Data.Collection.GetOrCreate(typeof(PresentationRhythmEngineSystemStart));
						simulationApplication.Data.Collection.GetOrCreate(typeof(ShoutDrumSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(OnNewBeatSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(BgmDefaultDirectorCommandSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(BgmDefaultDirectorSoundtrackSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(LoadActiveBgmSystem));
					}, default);
				}

			global.Collection.GetOrCreate(typeof(BgmManager));
		}
	}
}