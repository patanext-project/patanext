using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[UpdateAfter(typeof(ApplyCommandEngineSystem))]
	public class RhythmEngineResizeCommandBufferSystem : RhythmEngineSystemBase
	{
		public RhythmEngineResizeCommandBufferSystem(WorldCollection collection) : base(collection)
		{
		}

		public override void OnRhythmEngineSimulationPass()
		{
			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<RhythmEngineLocalCommandBuffer>(),
				GameWorld.AsComponentType<RhythmEngineLocalState>(),
				GameWorld.AsComponentType<RhythmEngineSettings>()
			}))
			{
				var progressionBuffer = GameWorld.GetBuffer<RhythmEngineLocalCommandBuffer>(entity)
				                                 .Reinterpret<FlowPressure>();
				ref readonly var state    = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);
				ref readonly var settings = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);
				var              flowBeat = RhythmEngineUtility.GetFlowBeat(state, settings);
				var              mercy    = 0; // todo: when on authoritative server, increment it by one
				for (var j = 0; j != progressionBuffer.Count; j++)
				{
					var currCommand = progressionBuffer[j];
					if (flowBeat >= currCommand.FlowBeat + mercy + settings.MaxBeat
					    || state.IsRecovery(flowBeat))
					{
						progressionBuffer.RemoveAt(j--);
					}
				}
			}
		}
	}
}