using System;
using System.Collections.Generic;
using DefaultEcs;
using PataNext.Module.RhythmEngine.Data;

namespace PataNext.Module.RhythmEngine
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
				else
					throw new InvalidOperationException("The end of sliders shouldn't be the start of pressure");
			}

			return array.Slice(0, resultCount);
		}

		public static bool CanBePredicted(IList<RhythmCommandAction>               commandTarget,
		                                  ReadOnlySpan<ComputedSliderFlowPressure> executingCommand,
		                                  TimeSpan                                 beatInterval)
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
				if (!action.Beat.IsValid(executingCommand[i], startSpan, beatInterval))
					return false;
			}

			return true;
		}

		public static bool CanBePredicted<TCommandList>(IList<RhythmCommandAction> commandTarget,
		                                                TCommandList               executingCommand,
		                                                TimeSpan                   beatInterval)
			where TCommandList : IList<FlowPressure>
		{
			var computedSpan = computeFlowPressures(stackalloc ComputedSliderFlowPressure[executingCommand.Count], executingCommand);
			return CanBePredicted(commandTarget, computedSpan, beatInterval);
		}

		public static bool SameAsSequence(IList<RhythmCommandAction>               commandTarget,
		                                  ReadOnlySpan<ComputedSliderFlowPressure> executingCommand,
		                                  TimeSpan                                 beatInterval)
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

		public static void GetCommand<TCommandList, TOutputEntityList>(EntitySet    set,
		                                                               TCommandList executingCommand, TOutputEntityList commandsOutput,
		                                                               bool         isPredicted,      TimeSpan          beatInterval)
			where TCommandList : IList<FlowPressure>
			where TOutputEntityList : IList<Entity>
		{
			var computedSpan = computeFlowPressures(stackalloc ComputedSliderFlowPressure[executingCommand.Count], executingCommand);
			foreach (ref readonly var entity in set.GetEntities())
			{
				if (!entity.Has<RhythmCommandDefinition>())
					throw new InvalidOperationException("This EntitySet was malformed");

				var definition = entity.Get<RhythmCommandDefinition>();
				if (!isPredicted && SameAsSequence(definition.Actions, computedSpan, beatInterval))
				{
					commandsOutput.Add(entity);
					return;
				}

				if (isPredicted && CanBePredicted(definition.Actions, executingCommand, beatInterval))
					commandsOutput.Add(entity);
			}
		}
	}
}