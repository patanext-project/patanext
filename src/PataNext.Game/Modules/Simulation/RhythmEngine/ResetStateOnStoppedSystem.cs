using PataNext.Game.Modules.Simulation.RhythmEngine.Components;
using revecs;
using revecs.Systems;

namespace PataNext.Game.Modules.Simulation.RhythmEngine;

public partial struct ResetStateOnStoppedSystem : ISystem
{
    [RevolutionSystem]
    [DependOn(typeof(ApplyTagsSystem))]
    [DependOn(typeof(ProcessSystem))]
    private static void Method([Query] q<
        Write<GameComboState>,
        Write<RhythmEngineRecoveryState>,
        
        None<RhythmEngineIsPlaying>,
        None<RhythmEngineIsPaused>
    > engines)
    {
        foreach (var (combo, recovery) in engines)
        {
            combo.__ref = default;
            recovery.__ref = default;
        }
    }
}