using System;
using System.Collections.Generic;
using DefaultEcs;
using PataNext.Module.RhythmEngine.Data;

namespace PataNext.Module.RhythmEngine
{
	public static class RhythmCommandUtility
	{
		public static bool CanBePredicted<TCommandList>(IList<RhythmCommandAction> commandTarget, TCommandList currentCommand)
			where TCommandList : IList<FlowPressure>
		{
			if (currentCommand.Count == 0)
				return true; // an empty command is valid

			var firstBeat = currentCommand[0].FlowBeat;
			for (int seq = 0, curr = 0; curr < currentCommand.Count; curr++)
			{
				if (!commandTarget[seq].ContainsInRange(currentCommand[curr].FlowBeat - firstBeat))
				{
					return false;
				}

				if (commandTarget[seq].Key != currentCommand[curr].KeyId)
					return false;

				if (seq > 0
				    && commandTarget[seq].AllowedInterval > TimeSpan.Zero
				    && (currentCommand[seq].Time - currentCommand[seq - 1].Time) * 0.001f >= commandTarget[seq].AllowedInterval)
				{
					return false;
				}

				seq++;
			}

			return true;
		}

		public static bool SameAsSequence<TCommandList>(IList<RhythmCommandAction> commandTarget, TCommandList executingCommand)
			where TCommandList : IList<FlowPressure>
		{
			if (executingCommand.Count <= 0)
				return false;

			var lastCommandBeat = executingCommand[^1].FlowBeat;
			var commandLength   = commandTarget[^1].BeatRange.End - commandTarget[0].BeatRange.Start;
			var startBeat       = lastCommandBeat - commandLength;

			// 1. Ignore this command since the target has more pressures than what we have for now.
			if (executingCommand.Count < commandTarget.Count)
				return false;

			var comDiff = executingCommand.Count - commandTarget.Count;
			// 2. Our commands has way more pressures than required, ignore it.
			if (comDiff < 0)
				return false;

			// 3. If it can't be even predicted, ignore it.
			if (!CanBePredicted(commandTarget, executingCommand))
				return false;

			for (var com = commandTarget.Count - 1; com >= 0; com--)
			{
				var range = commandTarget[com].BeatRange;
				range.Start += startBeat;
				range.End += startBeat;

				var comBeat = executingCommand[com + comDiff].FlowBeat;

				if (commandTarget[com].Key != executingCommand[com + comDiff].KeyId)
					return false;

				if (!(range.Start <= comBeat && comBeat <= range.End))
					return false;
			}

			return true;
		}

		public static void GetCommand<TCommandList, TOutputEntityList>(EntitySet set, TCommandList executingCommand, TOutputEntityList commandsOutput, bool isPredicted)
			where TCommandList : IList<FlowPressure>
			where TOutputEntityList : IList<Entity>
		{
			foreach (ref readonly var entity in set.GetEntities())
			{
				if (!entity.Has<RhythmCommandDefinition>())
					throw new InvalidOperationException("This EntitySet was malformed");

				var definition = entity.Get<RhythmCommandDefinition>();
				if (!isPredicted && SameAsSequence(definition.Actions, executingCommand))
				{
					commandsOutput.Add(entity);
					return;
				}

				if (isPredicted && CanBePredicted(definition.Actions, executingCommand))
					commandsOutput.Add(entity);
			}
		}
	}
}