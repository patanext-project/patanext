using DefaultEcs;
using DefaultEcs.System;
using GameHost.Core.Ecs;
using PataponGameHost.RhythmEngine.Components;

namespace PataNext.Module.RhythmEngine.Systems
{
	public class ResetStateWhenStoppedSystem : RhythmEngineSystemBase
	{
		private ASystem system;

		public ResetStateWhenStoppedSystem(WorldCollection collection) : base(collection)
		{
			system = new ASystem(World.Mgr.GetEntities()
			                          .With<RhythmEngineController>()
			                          .With<GameCommandState>()
			                          .WithGameCombo()
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
				if (controller.State != EngineControllerState.Stopped)
					return;

				entity.Set<GameCombo.State>();
				entity.Set<GameCommandState>();
			}
		}
	}
}