using DefaultEcs;
using GameHost.Core.Ecs;

namespace PataNext.Module.RhythmEngine
{
	public class ManageComponentTagSystem : RhythmEngineSystemBase
	{
		private EntitySet engineSet;

		protected override void OnInit()
		{
			base.OnInit();
			engineSet = World.Mgr.GetEntities()
			                 .With<RhythmEngineController>()
			                 .AsSet();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			foreach (ref readonly var entity in engineSet.GetEntities())
			{
				ref readonly var controller = ref entity.Get<RhythmEngineController>();
				if (controller.State == EngineControllerState.Playing)
					entity.Set(new RhythmEngineIsPlaying());
				else
					entity.Remove<RhythmEngineIsPlaying>();
			}
		}

		public ManageComponentTagSystem(WorldCollection collection) : base(collection)
		{
		}
	}
}