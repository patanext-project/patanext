using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[UpdateAfter(typeof(ProcessEngineSystem))]
	public class OnInputForRhythmEngine : RhythmEngineSystemBase
	{
		public OnInputForRhythmEngine(WorldCollection collection) : base(collection)
		{
		}

		public override void OnRhythmEngineSimulationPass()
		{
			if (!GameWorld.TryGetSingleton(out GameTime gameTime))
				return;
			
			// solo only for now
			if (!GameWorld.TryGetSingleton(out PlayerInputComponent playerInput))
				return;

			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<RhythmEngineIsPlaying>(),
				GameWorld.AsComponentType<RhythmEngineController>(),
				GameWorld.AsComponentType<RhythmEngineLocalState>(),
				GameWorld.AsComponentType<RhythmEngineSettings>(),
				GameWorld.AsComponentType<RhythmEngineLocalCommandBuffer>()
			}))
			{
				ref var state = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);
				ref readonly var settings = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);
				ref var executing = ref GameWorld.GetComponentData<GameCommandState>(entity);
				
				var renderBeat = RhythmEngineUtility.GetFlowBeat(state, settings);

				var progressionBuffer = GameWorld.GetBuffer<RhythmEngineLocalCommandBuffer>(entity);
				var predictedBuffer   = GameWorld.GetBuffer<RhythmEnginePredictedCommandBuffer>(entity);
				for (var i = 0; i < playerInput.Actions.Length; i++)
				{
					ref readonly var action = ref playerInput.Actions[i];
					if (!action.InterFrame.AnyUpdate(gameTime.Frame))
						continue;

					// If this is not the end of a slider or if it is but our command buffer is empty, skip it.
					if (action.InterFrame.HasBeenReleased(gameTime.Frame) && (!action.IsSliding || progressionBuffer.Span.Length == 0))
						continue;

					var cmdChainEndFlow = RhythmEngineUtility.GetFlowBeat(TimeSpan.FromMilliseconds(executing.ChainEndTimeMs), settings.BeatInterval);
					var cmdEndFlow      = RhythmEngineUtility.GetFlowBeat(TimeSpan.FromMilliseconds(executing.EndTimeMs), settings.BeatInterval);
					
					// check for one beat space between inputs (should we just check for predicted commands? 'maybe' we would have a command with one beat space)
					var failFlag1 = progressionBuffer.Count > 0
					                && predictedBuffer.Count == 0
					                && renderBeat > progressionBuffer[^1].Value.FlowBeat + 1
					                && cmdChainEndFlow > 0;
					// check if this is the first input and was started after the command input time
					var failFlag3 = renderBeat > cmdEndFlow
					                && progressionBuffer.Count == 0
					                && cmdEndFlow > 0;
					// check for inputs that were done after the current command chain
					var failFlag2 = renderBeat >= cmdChainEndFlow
					                && cmdChainEndFlow > 0;
					failFlag2 = false; // this flag is deactivated for delayed reborn ability
					var failFlag0 = cmdEndFlow > renderBeat && cmdEndFlow > 0;

					if (failFlag0 || failFlag1 || failFlag2 || failFlag3)
					{
						state.RecoveryActivationBeat = renderBeat + 1;
						executing                    = default;
						continue;
					}
					
					var pressure = new FlowPressure(i + 1, state.Elapsed, settings.BeatInterval)
					{
						IsSliderEnd = action.IsSliding
					};

					progressionBuffer.Add(new RhythmEngineLocalCommandBuffer {Value = pressure});
					state.LastPressure = pressure;
				}
			}
		}
	}
}