using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Native;
using GameHost.Native.Fixed;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Resources;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[UpdateAfter(typeof(GetNextCommandEngineSystem))]
	public class ApplyCommandEngineSystem : RhythmEngineSystemBase
	{
		public ApplyCommandEngineSystem(WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery entityQuery;
		
		public override void OnRhythmEngineSimulationPass()
		{
			var commandSetBuffer = new FixedBuffer128<GameEntityHandle>();
			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<RhythmCommandResource>(),
				GameWorld.AsComponentType<RhythmCommandActionBuffer>()
			}))
			{
				if (commandSetBuffer.GetCapacity() > commandSetBuffer.GetLength())
					commandSetBuffer.Add(entity);
				else
					Console.WriteLine("couldn't add more commands!");
			}

			if (commandSetBuffer.GetLength() == 0)
				return;

			foreach (var entity in entityQuery ??= CreateEntityQuery(new[]
			{
				GameWorld.AsComponentType<RhythmEngineIsPlaying>(),
				GameWorld.AsComponentType<RhythmEngineSettings>(),
				GameWorld.AsComponentType<RhythmEngineLocalState>(),
				GameWorld.AsComponentType<RhythmEngineExecutingCommand>(),
				GameWorld.AsComponentType<RhythmEngineCommandProgressBuffer>(),
				GameWorld.AsComponentType<RhythmEnginePredictedCommandBuffer>(),
				GameWorld.AsComponentType<GameCombo.Settings>(),
				GameWorld.AsComponentType<GameCombo.State>(),

				GameWorld.AsComponentType<SimulationAuthority>(),
			}))
			{
				ref readonly var state         = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);
				ref readonly var comboSettings = ref GameWorld.GetComponentData<GameCombo.Settings>(entity);
				ref var          settings      = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);
				ref var          comboState    = ref GameWorld.GetComponentData<GameCombo.State>(entity);
				ref var          commandState  = ref GameWorld.GetComponentData<GameCommandState>(entity);

				if (!state.CanRunCommands)
				{
					commandState.Reset();
					continue;
				}

				var predictedCommandBuffer = GameWorld.GetBuffer<RhythmEnginePredictedCommandBuffer>(entity);

				// TODO: Apply Ability Selection

				const int mercy    = 1; // increase it by one on a server
				const int cmdMercy = 0; // increase it by three on a server

				ref var executing = ref GameWorld.GetComponentData<RhythmEngineExecutingCommand>(entity);

				var rhythmActiveAtFlowBeat = executing.ActivationBeatStart;

				var checkStopBeat = Math.Max(state.LastPressure.FlowBeat + mercy,
					RhythmEngineUtility.GetFlowBeat(new TimeSpan(commandState.EndTimeMs * TimeSpan.TicksPerMillisecond), settings.BeatInterval) + cmdMercy);
				if (true) // todo: !isServer && simulateTagFromEntity.Exists(entity)
				{
					checkStopBeat = Math.Max(checkStopBeat,
						RhythmEngineUtility.GetFlowBeat(new TimeSpan(commandState.EndTimeMs * TimeSpan.TicksPerMillisecond), settings.BeatInterval));
				}

				var flowBeat       = RhythmEngineUtility.GetFlowBeat(state, settings);
				var activationBeat = RhythmEngineUtility.GetActivationBeat(state, settings);
				if (state.IsRecovery(flowBeat)
				    || (rhythmActiveAtFlowBeat < flowBeat && checkStopBeat < activationBeat)
				    || (executing.CommandTarget == default && predictedCommandBuffer.Count != 0 && rhythmActiveAtFlowBeat < state.LastPressure.FlowBeat)
				    || (predictedCommandBuffer.Count == 0))
				{
					/*Console.WriteLine($"0 => {state.IsRecovery(flowBeat)} ({flowBeat})");
					Console.WriteLine($"1 => {(rhythmActiveAtFlowBeat < flowBeat && checkStopBeat < activationBeat)} ({flowBeat})");
					Console.WriteLine($"2 => {(executing.CommandTarget == default && predictedCommandBuffer.Count != 0 && rhythmActiveAtFlowBeat < state.LastPressure.FlowBeat)} ({flowBeat})");
					Console.WriteLine($"3 => {(predictedCommandBuffer.Count == 0)} ({flowBeat})");*/

					comboState = default;
					commandState.Reset();
					executing = default;
				}

				if (executing.CommandTarget == default || state.IsRecovery(flowBeat))
				{
					commandState.Reset();
					comboState = default;
					executing  = default;
					continue;
				}

				if (!executing.WaitingForApply)
					continue;
				executing.WaitingForApply = false;

				ref var identifier   = ref GameWorld.GetComponentData<RhythmCommandIdentifier>(executing.CommandTarget.Handle);
				var     beatDuration = identifier.Duration;
				/*foreach (var element in targetResourceBuffer.Span)
					beatDuration = Math.Max(beatDuration, (int) Math.Ceiling(element.Value.Beat.Target + 1 + element.Value.Beat.Offset + element.Value.Beat.SliderLength));*/

				// if (!isServer && settings.UseClientSimulation && simulateTagFromEntity.Exists(entity))
				if (true)
				{
					commandState.ChainEndTimeMs = (int) ((rhythmActiveAtFlowBeat + beatDuration + 4) * (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
					commandState.StartTimeMs    = (int) (executing.ActivationBeatStart * (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
					commandState.EndTimeMs      = (int) (executing.ActivationBeatEnd * (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));

					if (TryGetComponentData(entity, out Relative<PlayerDescription> relativePlayer)
					    && TryGetComponentData(relativePlayer.Handle, out GameRhythmInputComponent playerInputComponent))
					{
						commandState.Selection = playerInputComponent.Ability;
					}

					var wasFever = comboSettings.CanEnterFever(comboState);

					comboState.Count++;
					comboState.Score += (float) (executing.Power - 0.5) * 2;
					if (comboState.Score < 0)
						comboState.Score = 0;

					// We have a little bonus when doing a perfect command
					if (executing.IsPerfect
					    && wasFever
					    && HasComponent(entity, AsComponentType<RhythmSummonEnergy>()))
					{
						GetComponentData<RhythmSummonEnergy>(entity).Value += 20;
					}
				}
			}
		}
	}
}