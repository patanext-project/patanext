using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineRecoveryState : ISparseComponent
{
    public int RecoveryActivationBeat;

    public readonly bool IsRecovery(int activationBeat)
    {
        return RecoveryActivationBeat > activationBeat;
    }
}