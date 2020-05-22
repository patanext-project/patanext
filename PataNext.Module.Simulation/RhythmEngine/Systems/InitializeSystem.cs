using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Entities;
using PataponGameHost.RhythmEngine.Components;

namespace PataNext.Module.RhythmEngine
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	public class InitializeSystem : AppSystem
	{
		private EntitySet engineInputSet;
		private EntitySet engineOnNewBeatSet;

		public InitializeSystem(WorldCollection collection) : base(collection)
		{
			engineInputSet = collection.Mgr.GetEntities()
			                           .With<RhythmEnginePressureInput>()
			                           .AsSet();
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