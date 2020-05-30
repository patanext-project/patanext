using System;
using PataNext.Module.RhythmEngine;

namespace PataNext.Module.RhythmEngine.Data
{
	public struct ComputedSliderFlowPressure
	{
		public bool IsSlider => End.IsSliderEnd;
		
		public FlowPressure Start, End;
	}

	public struct FlowPressure
	{
		public const float  Error   = 0.99f;
		public const double Perfect = 0.2f;

		/// <summary>
		/// Is this the end of a slider?
		/// </summary>
		public bool IsSliderEnd;
		
		/// <summary>
		///     Our custom Rhythm Key (Pata 1, Pon 2, Don 3, Chaka 4)
		/// </summary>
		public int KeyId;

		public int FlowBeat;

		/// <summary>
		///     The time of the beat, it will be used to do server side check
		/// </summary>
		public TimeSpan Time;

		/// <summary>
		///     The score of the pressure [range -1 - 1, where 0 is perfect]
		/// </summary>
		/// <example>
		///     Let's say we made an engine with BeatInterval = 0.5f.
		///     The current time is 14.2f.
		///     The actual beat is timed at 14f.
		///     The score is 0.2f.
		///     If we made one at 13.8f, the score should be the same (but negative)!
		/// </example>
		public float Score;

		public FlowPressure(int keyId, TimeSpan time, TimeSpan beatInterval)
		{
			FlowBeat = RhythmEngineUtility.GetFlowBeat(time, beatInterval);
			Score    = RhythmEngineUtility.GetScore(time, beatInterval);

			KeyId = keyId;
			Time  = time;

			IsSliderEnd = false;
		}

		public float GetAbsoluteScore()
		{
			return MathF.Abs(Score);
		}
	}
}