using System;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures
{

	public struct Beat
	{
		public int Target;

		private double offset;
		private int    sliderLength;

		public double Offset
		{
			get => offset;
			set => offset = Math.Clamp(value, -1, 1);
		}

		/// <summary>
		/// How long is this beat? (in beats)
		/// </summary>
		public int SliderLength
		{
			get => sliderLength;
			set => sliderLength = Math.Max(0, value);
		}

		private static float unlerp(TimeSpan a, TimeSpan b, TimeSpan x)
		{
			if (a != b)
				return (float) (x.Ticks - a.Ticks) / (b.Ticks - a.Ticks);
			return 0.0f;
		}

		private static bool isValid(int beat, double offset, TimeSpan commandStart, TimeSpan elapsed, TimeSpan beatInterval, float scoreLimit = 0.6f)
		{
			elapsed -= commandStart;

			var targetTimed = beat * beatInterval;
			var targetStart = targetTimed + beatInterval * Math.Clamp(offset, -0.9, +0.9);
			var score       = Math.Abs(unlerp(targetStart, targetStart + beatInterval, elapsed));

			return Math.Abs(score) < Math.Min(1, scoreLimit);
		}

		public bool IsStartValid(TimeSpan commandStart, TimeSpan elapsed, TimeSpan beatInterval)
		{
			return isValid(Target, offset, commandStart, elapsed, beatInterval);
		}

		public bool IsSliderValid(TimeSpan commandStart, TimeSpan elapsed, TimeSpan beatInterval)
		{
			return isValid(Target + SliderLength, offset, commandStart, elapsed, beatInterval);
		}

		public bool IsValid(ComputedSliderFlowPressure computed, TimeSpan start, TimeSpan beatInterval)
		{
			return IsStartValid(start, computed.Start.Time, beatInterval)
			       && (!computed.IsSlider && sliderLength == 0 || IsSliderValid(start, computed.End.Time, beatInterval));
		}

		public bool IsPredictionValid(ComputedSliderFlowPressure computed, TimeSpan start, TimeSpan beatInterval)
		{
			return IsStartValid(start, computed.Start.Time, beatInterval)
			       && (!computed.IsSlider || sliderLength > 0 && IsSliderValid(start, computed.End.Time, beatInterval));
		}
	}

	/// <summary>
	/// An action that should be attached to a <see cref="RhythmCommandDefinition"/> collection.
	/// </summary>
	public struct RhythmCommandAction
	{
		public Beat Beat;

		/// <summary>
		/// The key required for this action to success
		/// </summary>
		public int Key;

		public RhythmCommandAction(int beat, int key)
		{
			Beat = new Beat {Target = beat};
			Key  = key;
		}

		public RhythmCommandAction(Beat beat, int key)
		{
			Beat = beat;
			Key  = key;
		}

		public override string ToString()
		{
			return $"(K={Key} {Beat.Target}.{Beat.Offset}-->{Beat.Target + Beat.SliderLength}.{Beat.Offset})";
		}

		public static RhythmCommandAction With(int beatTarget, int key)
		{
			return new RhythmCommandAction(beatTarget, key);
		}

		public static RhythmCommandAction WithOffset(int beatTarget, float offset, int key)
		{
			return new RhythmCommandAction(new Beat {Target = beatTarget, Offset = offset}, key);
		}

		public static RhythmCommandAction WithSlider(int beatTarget, int sliderLength, int key)
		{
			return new RhythmCommandAction(new Beat {Target = beatTarget, SliderLength = sliderLength}, key);
		}

		public static RhythmCommandAction WithOffsetAndSlider(int beatTarget, float offset, int sliderLength, int key)
		{
			return new RhythmCommandAction(new Beat {Target = beatTarget, Offset = offset, SliderLength = sliderLength}, key);
		}
	}
}