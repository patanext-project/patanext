using DefaultEcs;
using DefaultEcs.System;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using PataponGameHost.RhythmEngine.Components;

namespace PataNext.Module.Simulation.RhythmEngine.Systems
{
	public class ResetStateWhenStoppedSystem : RhythmEngineSystemBase
	{
		private ASystem system;

		public ResetStateWhenStoppedSystem(WorldCollection collection) : base(collection)
		{
			system = new ASystem(World.Mgr.GetEntities()
			                          .With<RhythmEngineController>()
			                          .With<GameComboState>()
			                          .With<GameCommandState>()
			                          .AsSet());
		}

		protected override void OnUpdate() => system.Update(default);

		private class ASystem : AEntitySystem<float>
		{
			public ASystem(EntitySet set) : base(set)
			{
			}

			protected override void Update(float _, in Entity entity)
			{
				ref readonly var controller = ref entity.Get<RhythmEngineController>();
				if (controller.State != RhythmEngineState.Stopped)
					return;

				entity.Get<GameComboState>()   = default;
				entity.Get<GameCommandState>() = default;
			}
		}
	}
}