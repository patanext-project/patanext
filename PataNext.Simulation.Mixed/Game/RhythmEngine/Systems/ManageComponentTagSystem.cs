using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	public class ManageComponentTagSystem : RhythmEngineSystemBase
	{
		public ManageComponentTagSystem(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[] {GameWorld.AsComponentType<RhythmEngineController>()}))
			{
				ref readonly var controller = ref GameWorld.GetComponentData<RhythmEngineController>(entity);
				if (controller.State == RhythmEngineState.Playing)
					GameWorld.AddComponent<RhythmEngineIsPlaying>(entity);
				else
					GameWorld.RemoveComponent(entity, GameWorld.AsComponentType<RhythmEngineIsPlaying>());
			}
		}
	}
}