using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct GameCommandState : ISparseComponent
{
    public int StartTimeMs;
    public int EndTimeMs;
    public int ChainEndTimeMs;

    public TimeSpan StartTime => TimeSpan.FromMilliseconds(StartTimeMs);
    public TimeSpan EndTime => TimeSpan.FromMilliseconds(EndTimeMs);
    public TimeSpan ChainEndTime => TimeSpan.FromMilliseconds(ChainEndTimeMs);
    
    public void Reset()
    {
        StartTimeMs = EndTimeMs = ChainEndTimeMs = -1;
    }
		
    public readonly bool IsGamePlayActive(int milliseconds)
    {
        return milliseconds >= StartTimeMs && milliseconds <= EndTimeMs;
    }

    public readonly bool IsInputActive(int milliseconds, int beatInterval)
    {
        return milliseconds >= EndTimeMs - beatInterval && milliseconds <= EndTimeMs + beatInterval;
    }

    public readonly bool HasActivity(int milliseconds, int beatInterval)
    {
        return IsGamePlayActive(milliseconds)
               || IsInputActive(milliseconds, beatInterval);
    }

    public readonly bool HasActivity(RhythmEngineState state, RhythmEngineSettings settings)
    {
        return HasActivity((int) (state.Elapsed.Ticks / TimeSpan.TicksPerMillisecond), (int) (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
    }
}