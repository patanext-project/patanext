using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Simulation.Utility.Resource.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Resources.Keys;

namespace PataNext.Module.Simulation.Game.RhythmEngine
{
	public static class RhythmCommandUtility
	{
		private static ReadOnlySpan<ComputedSliderFlowPressure> computeFlowPressures<TCommandList>(Span<ComputedSliderFlowPressure> array, TCommandList executingCommand)
			where TCommandList : IList<FlowPressure>
		{
			var resultCount = 0;
			for (var exec = 0; exec != executingCommand.Count; exec++)
			{
				var pressure = executingCommand[exec];
				if (!pressure.IsSliderEnd)
				{
					array[resultCount].Start = pressure;
					if (exec + 1 < executingCommand.Count && executingCommand[exec + 1].IsSliderEnd)
					{
						array[resultCount].End = executingCommand[exec + 1];
						exec++;
					}

					resultCount++;
				}
			}

			return array.Slice(0, resultCount);
		}

		public static bool CanBePredicted<TCommandTarget>(TCommandTarget                           commandTarget,
		                                                  ReadOnlySpan<ComputedSliderFlowPressure> executingCommand,
		                                                  TimeSpan                                 beatInterval)
			where TCommandTarget : IList<RhythmCommandAction>
		{
			if (executingCommand.Length == 0)
				return true;

			if (executingCommand.Length > commandTarget.Count)
				return false;

			var startSpan = executingCommand[0].Start.FlowBeat * beatInterval;
			for (var i = 0; i != Math.Min(executingCommand.Length, commandTarget.Count); i++)
			{
				var action   = commandTarget[i];
				var pressure = executingCommand[i];

				if (action.Key != pressure.Start.KeyId)
					return false;
				if (!action.Beat.IsPredictionValid(executingCommand[i], startSpan, beatInterval))
					return false;
			}

			return true;
		}

		public static bool CanBePredicted<TCommandTarget, TCommandList>(TCommandTarget commandTarget,
		                                                                TCommandList   executingCommand,
		                                                                TimeSpan       beatInterval)
			where TCommandTarget : IList<RhythmCommandAction>
			where TCommandList : IList<FlowPressure>
		{
			var computedSpan = computeFlowPressures(stackalloc ComputedSliderFlowPressure[executingCommand.Count], executingCommand);
			return CanBePredicted(commandTarget, computedSpan, beatInterval);
		}

		public static bool SameAsSequence<TCommandTarget>(TCommandTarget                           commandTarget,
		                                                  ReadOnlySpan<ComputedSliderFlowPressure> executingCommand,
		                                                  TimeSpan                                 beatInterval)
			where TCommandTarget : IList<RhythmCommandAction>
		{
			if (executingCommand.Length != commandTarget.Count)
				return false;

			var startSpan = executingCommand[0].Start.FlowBeat * beatInterval;
			for (var i = 0; i != commandTarget.Count; i++)
			{
				var action   = commandTarget[i];
				var pressure = executingCommand[i];

				if (action.Key != pressure.Start.KeyId)
					return false;
				if (!action.Beat.IsValid(executingCommand[i], startSpan, beatInterval))
					return false;
			}

			return true;
		}

		public static bool SameAsSequence<TCommandList>(IList<RhythmCommandAction> commandTarget,
		                                                TCommandList               executingCommand,
		                                                TimeSpan                   beatInterval)
			where TCommandList : IList<FlowPressure>
		{
			var computedSpan = computeFlowPressures(stackalloc ComputedSliderFlowPressure[executingCommand.Count], executingCommand);
			return SameAsSequence(commandTarget, computedSpan, beatInterval);
		}

		public static void GetCommand<TCommandList, TOutputEntityList>(GameWorld    gameWorld,        Span<GameEntity>  entities,
		                                                               TCommandList executingCommand, TOutputEntityList commandsOutput,
		                                                               bool         isPredicted,      TimeSpan          beatInterval)
			where TCommandList : IList<FlowPressure>
			where TOutputEntityList : IList<GameResource<RhythmCommandResource>>
		{
			var computedSpan = computeFlowPressures(stackalloc ComputedSliderFlowPressure[executingCommand.Count], executingCommand);
			foreach (ref readonly var entity in entities)
			{
				if (!gameWorld.HasComponent<RhythmCommandActionBuffer>(entity))
					throw new InvalidOperationException($"#{entity.Id} has no actionbuffer");

				var actionBuffer = gameWorld.GetBuffer<RhythmCommandActionBuffer>(entity).Reinterpret<RhythmCommandAction>();
				if (!isPredicted && SameAsSequence(actionBuffer, computedSpan, beatInterval))
				{
					commandsOutput.Add(new GameResource<RhythmCommandResource>(entity));
					return;
				}

				if (isPredicted && CanBePredicted(actionBuffer, executingCommand, beatInterval))
					commandsOutput.Add(new GameResource<RhythmCommandResource>(entity));
			}
		}
	}
}