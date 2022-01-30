using Quadrum.Game.Modules.Simulation.Application;
using revecs.Systems.Generator;
using revghost;
using revghost.Module;

namespace PataNext.Game.Modules.RhythmEngine;

/*
 * Rhythm Engine Module:
 * - Create default commands in the simulation context
 * - Remove Confirm/Cancel buttons (commands are automatically executed when it find one)
 */

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            domain.SystemGroup.Add(new CreateCommandSystem());
        });
    }
}