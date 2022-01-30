using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Module;
using revghost.Utility;

namespace PataNext.Game.Client;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            Disposables.AddRange(new IDisposable[]
            {
                new UpdatePresentationSystems(domain.Scope)
            });
        });
    }
}