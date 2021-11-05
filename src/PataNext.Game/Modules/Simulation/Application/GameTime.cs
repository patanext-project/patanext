using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Simulation.Application;

public partial struct GameTime : ISparseComponent
{
    public int Frame;
    public TimeSpan Total;
    public TimeSpan Delta;
}