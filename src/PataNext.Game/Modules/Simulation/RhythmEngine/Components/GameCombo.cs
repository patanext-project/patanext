using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct GameComboSettings : ISparseComponent
{
    public int MaxComboToReachFever;
    public float RequiredScoreStart;
    public float RequiredScoreStep;
    
    public readonly bool CanEnterFever(int combo, float score)
    {
        return combo > MaxComboToReachFever
               || RequiredScoreStart - combo * RequiredScoreStep < score;
    }

    public readonly bool CanEnterFever(GameComboState state)
    {
        return CanEnterFever(state.Count, state.Score);
    }
}

public partial struct GameComboState : ISparseComponent
{
    public int Count;
    public int Score;
}