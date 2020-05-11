using System;

namespace PataNext.Module.Simulation.RhythmEngine
{
	public static class RhythmEngineUtility
	{
		public static int GetActivationBeat(in TimeSpan elapsed, in TimeSpan interval)
		{
			if (elapsed == TimeSpan.Zero || interval == TimeSpan.Zero)
				return 0;
			return (int) (elapsed.Ticks / interval.Ticks) + (elapsed < TimeSpan.Zero ? -1 : 0);
		}
		
		public static int GetActivationBeat<TState>(in TState state, in RhythmEngineSettings settings)
			where TState : IRhythmEngineState =>
			GetActivationBeat(state.Elapsed, settings.BeatInterval);
	}
}