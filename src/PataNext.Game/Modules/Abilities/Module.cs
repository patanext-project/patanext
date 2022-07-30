using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revghost;
using revghost.Module;

namespace PataNext.Game.Modules.Abilities;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            domain.Scope.Context.Register(new AbilitySpawner(domain.Scope));
        });
    }
}