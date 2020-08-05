using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Native;
using GameHost.Native.Fixed;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Resources.Keys;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[UpdateAfter(typeof(GetNextCommandEngineSystem))]
	public class ApplyCommandEngineSystem : RhythmEngineSystemBase
	{
		public ApplyCommandEngineSystem(WorldCollection collection) : base(collection)
		{
		}

		public override void OnRhythmEngineSimulationPass()
		{
			var commandSetBuffer = new FixedBuffer128<GameEntity>();
			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<GameResourceKey<RhythmCommandResourceKey>>(),
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

			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<RhythmEngineIsPlaying>(),
				GameWorld.AsComponentType<RhythmEngineSettings>(),
				GameWorld.AsComponentType<RhythmEngineLocalState>(),
				GameWorld.AsComponentType<RhythmEngineExecutingCommand>(),
				GameWorld.AsComponentType<RhythmEngineLocalCommandBuffer>(),
				GameWorld.AsComponentType<RhythmEnginePredictedCommandBuffer>(),
			}))
			{
				ref readonly var state        = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);
				ref var          settings     = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);
				ref var          comboState   = ref GameWorld.GetComponentData<GameCombo.State>(entity);
				ref var          commandState = ref GameWorld.GetComponentData<GameCommandState>(entity);

				if (!state.CanRunCommands)
				{
					commandState.Reset();
					return;
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
					return;
				}

				if (!executing.WaitingForApply)
					return;
				executing.WaitingForApply = false;

				var targetResourceBuffer = GameWorld.GetBuffer<RhythmCommandActionBuffer>(executing.CommandTarget.Entity);
				var beatDuration         = 0;
				foreach (var element in targetResourceBuffer.Span)
					beatDuration = Math.Max(beatDuration, (int) Math.Ceiling(element.Value.Beat.Target + 1 + element.Value.Beat.Offset + element.Value.Beat.SliderLength));

				// if (!isServer && settings.UseClientSimulation && simulateTagFromEntity.Exists(entity))
				if (true)
				{
					commandState.ChainEndTimeMs = (int) ((rhythmActiveAtFlowBeat + beatDuration + 4) * (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
					commandState.StartTimeMs    = (int) (executing.ActivationBeatStart * (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
					commandState.EndTimeMs      = (int) (executing.ActivationBeatEnd * (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));

					comboState.Count++;
					comboState.Score += (float) (executing.Power - 0.5) * 2;
					if (comboState.Score < 0)
						comboState.Score = 0;
					Console.WriteLine($"Score={comboState.Score}, Power={executing.Power}");
				}
			}
		}
	}
}