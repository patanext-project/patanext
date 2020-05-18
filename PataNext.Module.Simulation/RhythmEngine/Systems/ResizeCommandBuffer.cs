using System;
using DefaultEcs;
using DefaultEcs.System;
using DefaultEcs.Threading;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using PataponGameHost.RhythmEngine.Components;

namespace PataNext.Module.Simulation.RhythmEngine
{
	[UpdateAfter(typeof(ProcessCommandSystem))]
	public class RhythmEngineResizeCommandBufferSystem : RhythmEngineSystemBase
	{
		private System system;

		public RhythmEngineResizeCommandBufferSystem(WorldCollection collection) : base(collection)
		{
			system = new System(collection.Mgr.GetEntities()
			                              .With<RhythmEngineLocalCommandBuffer>()
			                              .With<RhythmEngineLocalState>()
			                              .With<RhythmEngineSettings>()
			                              .AsSet());
		}

		protected override void OnUpdate()
		{
			system.Update(0);
		}

		public class System : AEntitySystem<float>
		{
			public System(EntitySet set) : base(set, new DefaultParallelRunner(Processor.GetWorkerCount(0.5)))
			{
			}

			protected override void Update(float _, in Entity entity)
			{
				var progression = entity.Get<RhythmEngineLocalCommandBuffer>();
				var state       = entity.Get<RhythmEngineLocalState>();
				var settings    = entity.Get<RhythmEngineSettings>();
				var flowBeat    = RhythmEngineUtility.GetFlowBeat(state, settings);
				var mercy       = 0; // todo: when on authoritative server, increment it by one
				for (var j = 0; j != progression.Count; j++)
				{
					var currCommand = progression[j];
					if (flowBeat >= currCommand.FlowBeat + mercy + settings.MaxBeat
					    || state.IsRecovery(flowBeat))
					{
						progression.RemoveAt(j--);
					}
				}
			}
		}
	}
}