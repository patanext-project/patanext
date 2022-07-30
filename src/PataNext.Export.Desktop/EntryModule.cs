using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Module;

namespace PataNext.Export.Desktop;

public class EntryModule : HostModule
{
    public EntryModule(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        //LoadModule(sc => new PataNext.Game.Module(sc));
        //LoadModule(sc => new PataNext.Game.Client.Module(sc));

        LoadModule(sc => new Quadrum.Game.Modules.Simulation.Module(sc));
        
        TrackDomain((SimulationDomain domain) =>
        {
            _ = new TestSimulationSystem(domain.Scope);
        });
    }
}