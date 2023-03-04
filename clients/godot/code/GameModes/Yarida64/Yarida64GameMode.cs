using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.GameModes.Yarida64;

public partial struct Yarida64GameMode : ISparseComponent
{
    public int YaridaCount;
    public EPhase Phase;
    public int YaridaOvertakeCount;

    public enum EPhase
    {
        Waiting,
        March,
        Backward
    }
}