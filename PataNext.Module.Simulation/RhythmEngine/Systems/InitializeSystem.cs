using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Entities;

namespace PataNext.Module.Simulation.RhythmEngine
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	public class InitializeSystem : AppSystem
	{
		private EntitySet engineOnNewBeatSet;
		
		public InitializeSystem(WorldCollection collection) : base(collection)
		{
			engineOnNewBeatSet = collection.Mgr.GetEntities()
			                               .With<RhythmEngineController>()
			                               .With<RhythmEngineOnNewBeat>()
			                               .AsSet();
		}

		protected override void OnUpdate()
		{
			engineOnNewBeatSet.Remove<RhythmEngineOnNewBeat>();
		}
	}
}