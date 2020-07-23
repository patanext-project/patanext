using System;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures
{

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
		public int BeatEnd   => BeatRange.End;

		public bool ContainsInRange(int beatVal)
		{
			return BeatRange.Start <= beatVal && BeatRange.End >= beatVal;
		}

		public override string ToString()
		{
			return $"(K={Key} {BeatStart}..{BeatEnd}, I={AllowedInterval})";
		}
	}
}