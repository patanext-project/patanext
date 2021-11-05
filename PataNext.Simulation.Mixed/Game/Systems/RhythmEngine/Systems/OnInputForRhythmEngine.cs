using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.Components;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[UpdateAfter(typeof(ProcessEngineSystem))]
	public class OnInputForRhythmEngine : RhythmEngineSystemBase
	{
		private NetReportTimeSystem reportTimeSystem;
		
		public OnInputForRhythmEngine(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref reportTimeSystem);
		}

		private EntityQuery query;

		public override void OnRhythmEngineSimulationPass()
		{
			var playerAccessor = GetAccessor<Relative<PlayerDescription>>();
			foreach (var entity in query ??= CreateEntityQuery(new[]
			{
				GameWorld.AsComponentType<RhythmEngineIsPlaying>(),
				GameWorld.AsComponentType<RhythmEngineController>(),
				GameWorld.AsComponentType<RhythmEngineLocalState>(),
				GameWorld.AsComponentType<RhythmEngineSettings>(),
				GameWorld.AsComponentType<RhythmEngineCommandProgressBuffer>(),
				GameWorld.AsComponentType<SimulationAuthority>(),
				
				GameWorld.AsComponentType<Relative<PlayerDescription>>()
			}))
			{
				if (!TryGetComponentData(playerAccessor[entity].Handle, out GameRhythmInputComponent input))
				{
					continue;
				}

				var reportTime = reportTimeSystem.Get(playerAccessor[entity].Handle, out _);

				ref var          state     = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);
				ref readonly var settings  = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);
				ref var          executing = ref GameWorld.GetComponentData<GameCommandState>(entity);

				var renderBeat = RhythmEngineUtility.GetFlowBeat(state, settings);
				if (renderBeat < 0)
					return;

				var progressionBuffer = GameWorld.GetBuffer<RhythmEngineCommandProgressBuffer>(entity);
				var predictedBuffer   = GameWorld.GetBuffer<RhythmEnginePredictedCommandBuffer>(entity);
				for (var i = 0; i < input.Actions.Length; i++)
				{
					ref readonly var action = ref input.Actions[i];
					if (!action.InterFrame.AnyUpdate(reportTime.Active))
						continue;

					// If this is not the end of a slider or if it is but our command buffer is empty, skip it.
					if (action.InterFrame.HasBeenReleased(reportTime.Active) && (!action.IsSliding || progressionBuffer.Span.Length == 0))
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

					if (Math.Abs(pressure.Score) <= FlowPressure.Perfect && HasComponent<RhythmSummonEnergy>(entity))
						GetComponentData<RhythmSummonEnergy>(entity).Value += 10;

					progressionBuffer.Add(new RhythmEngineCommandProgressBuffer {Value = pressure});
					state.LastPressure = pressure;
				}
			}
		}
	}
}