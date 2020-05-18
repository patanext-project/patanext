using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace PataNext.Module.Simulation.RhythmEngine
{
	public struct ActionRange
	{
		public int Start, End;

		public ActionRange(int start, int end)
		{
			Start = start;
			End   = end;
		}
	}

	/// <summary>
	/// An action that should be attached to a <see cref="RhythmCommandDefinition"/> collection.
	/// </summary>
	public struct RhythmCommandAction
	{
		/// <summary>
		/// How much this 
		/// </summary>
		public ActionRange BeatRange;

		/// <summary>
		/// The key required for this action to success
		/// </summary>
		public int Key;

		/// <summary>
		/// The maximum interval allowed between each action...
		/// </summary>
		public TimeSpan AllowedInterval;

		public RhythmCommandAction(int beat, int key)
		{
			BeatRange       = new ActionRange(beat, beat);
			Key             = key;
			AllowedInterval = TimeSpan.MinValue;
		}

		public RhythmCommandAction(int beat, int beatLength, int key)
		{
			BeatRange       = new ActionRange(beat, beat + beatLength);
			Key             = key;
			AllowedInterval = TimeSpan.MinValue;
		}

		public RhythmCommandAction(int beat, int beatLength, int key, TimeSpan allowedInterval)
		{
			BeatRange       = new ActionRange(beat, beat + beatLength);
			Key             = key;
			AllowedInterval = allowedInterval;
		}

		public int BeatStart => BeatRange.Start;
		public int BeatEnd => BeatRange.End;
		
		public bool ContainsInRange(int beatVal)
		{
			return BeatRange.Start <= beatVal && BeatRange.End >= beatVal;
		}

		public override string ToString()
		{
			return $"(K={Key} {BeatStart}..{BeatEnd}, I={AllowedInterval})";
		}
	}

	public class RhythmCommandDefinition
	{
		public          string                                   Identifier;
		public readonly ReadOnlyCollection<RhythmCommandAction> Actions;
		public readonly int Duration;

		private RhythmCommandDefinition()
		{
		}

		public RhythmCommandDefinition(string identifier, Span<RhythmCommandAction> sequences, int duration = 4)
		{
			Identifier = identifier;
			Actions    = Array.AsReadOnly(sequences.ToArray());
			Duration   = duration;
		}

		public override string ToString()
		{
			var str = $"Command: {Identifier} {{0}}";
			var cmdStr = string.Empty;
			for (var i = 0; i != Actions.Count; i++)
			{
				cmdStr += Actions[i].ToString();
				if (i + 1 < Actions.Count)
					cmdStr += ",";
			}

			return string.Format(str, cmdStr);
		}
	}
}