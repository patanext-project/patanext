using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineController : ISparseComponent
{
    public enum EState
    {
        Stopped = 0,
        Paused = 1,
        Playing = 2
    }

    public EState State;
    public TimeSpan StartTime;
}