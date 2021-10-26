using PataNext.Game.Modules.Simulation.RhythmEngine.Components;

namespace PataNext.Game.Modules.Simulation.RhythmEngine.Utility;

public static class RhythmEngineUtility
{
    public static int GetActivationBeat(in TimeSpan elapsed, in TimeSpan interval)
    {
        if (elapsed == TimeSpan.Zero || interval == TimeSpan.Zero)
            return 0;
        return (int) (elapsed.Ticks / interval.Ticks) + (elapsed < TimeSpan.Zero ? -1 : 0);
    }

    public static int GetFlowBeat(in TimeSpan elapsed, in TimeSpan interval)
    {
        if (elapsed == TimeSpan.Zero || interval == TimeSpan.Zero)
            return 0;

        var offsetTime = elapsed.Ticks + interval.Ticks * Math.Sign(interval.Ticks) / 2;
        if (offsetTime == 0)
            return 0;

        return (int) (offsetTime / interval.Ticks) + (offsetTime < 0 ? -1 : 0);
    }


    /// <summary>
    ///     Compute the score from a beat and time.
    /// </summary>
    /// <param name="elapsed">The elapsed time</param>
    /// <param name="interval">The interval between each beat</param>
    /// <returns></returns>
    public static float GetScore(TimeSpan elapsed, TimeSpan interval)
    {
        if (interval == TimeSpan.Zero || elapsed == TimeSpan.Zero)
            return 0;

        var beatTimeDelta = elapsed.Ticks % interval.Ticks;
        var halvedInterval = interval.Ticks * 0.5;
        var correctedTime = beatTimeDelta - halvedInterval;

        // this may happen if 'beatInterval' is 0
        if (double.IsNaN(correctedTime))
        {
            correctedTime = 0.0;
            if (interval == default)
                throw new InvalidOperationException(
                    $"{nameof(interval)} is set to 0, which is not allowed in FlowRhythmEngine.GetScore()");
        }

        return (float) ((correctedTime + -Math.Sign(correctedTime) * halvedInterval) / halvedInterval);
    }

    public static int GetActivationBeat(in RhythmEngineState state, in RhythmEngineSettings settings)
    {
        return GetActivationBeat(state.Elapsed, settings.BeatInterval);
    }

    public static int GetFlowBeat(in RhythmEngineState state, in RhythmEngineSettings settings)
    {
        return GetFlowBeat(state.Elapsed, settings.BeatInterval);
    }

    public static float GetScore(in RhythmEngineState state, in RhythmEngineSettings settings)
    {
        return GetScore(state.Elapsed, settings.BeatInterval);
    }
}