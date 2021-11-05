using DefaultEcs;
using revghost.Threading.V2;

namespace PataNext.Game.Modules.Simulation.Application;

/// <summary>
/// The current <see cref="SimulationDomain"/> used as the primary client.
/// </summary>
/// <remarks>
/// This will be used for rendering and sounds
/// </remarks>
public record struct CurrentSimulationClient(Entity Entity)
{
    public SimulationDomain Domain => (SimulationDomain) Entity.Get<IListener>();
}