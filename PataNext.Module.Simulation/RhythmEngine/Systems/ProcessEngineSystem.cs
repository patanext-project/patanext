using DefaultEcs;
using DefaultEcs.Command;
using DefaultEcs.System;
using DefaultEcs.Threading;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Entities;

namespace PataNext.Module.RhythmEngine
{
	[UpdateAfter(typeof(ManageComponentTagSystem))]
	public class ProcessEngineSystem : RhythmEngineSystemBase
	{
		private IManagedWorldTime worldTime;

		public ProcessEngineSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private System system;

		protected override void OnInit()
		{
			base.OnInit();
			system = new System(World.Mgr.GetEntities()
			                         .With<RhythmEngineIsPlaying>()
			                         .With<RhythmEngineController>()
			                         .With<RhythmEngineLocalState>()
			                         .With<RhythmEngineSettings>()
			                         .AsSet());
		}

		protected override void OnUpdate() => system.Update(worldTime);

		private class System : AEntitySystem<IManagedWorldTime>
		{
			private readonly World                 world;
			private readonly EntityCommandRecorder recorder;

			public System(EntitySet set) : base(set, new DefaultParallelRunner(Processor.GetWorkerCount(0.5)))
			{
				world    = set.World;
				recorder = new EntityCommandRecorder();
			}

			protected override void PreUpdate(IManagedWorldTime state) => recorder.Clear();

			protected override void Update(IManagedWorldTime wt, in Entity entity)
			{
				ref readonly var controller = ref entity.Get<RhythmEngineController>();
				ref readonly var settings   = ref entity.Get<RhythmEngineSettings>();
				ref var          state      = ref entity.Get<RhythmEngineLocalState>();

				// store the previous elapsed time, it will be used for checking new beats
				var previous      = state.Elapsed;
				var previousBeats = RhythmEngineUtility.GetActivationBeat(previous, settings.BeatInterval);

				state.Elapsed = wt.Total - controller.StartTime;

				var currentBeats = RhythmEngineUtility.GetActivationBeat(state, settings);
				if (previousBeats < currentBeats)
				{
					recorder.Record(entity)
					        .Set(new RhythmEngineOnNewBeat {Previous = previousBeats, Next = currentBeats});
				}
			}

			protected override void PostUpdate(IManagedWorldTime state) => recorder.Execute(world);

			public override void Dispose()
			{
				base.Dispose();
				recorder.Dispose();
			}
		}
	}
}