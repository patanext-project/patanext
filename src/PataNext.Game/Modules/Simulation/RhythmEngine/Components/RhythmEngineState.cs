using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineState : ISparseComponent
{
    public TimeSpan Elapsed;
    public TimeSpan PreviousStartTime;

    public int CurrentBeat;
    public uint NewBeatTick;

    public bool CanRunCommands => Elapsed > TimeSpan.Zero;
}