using DefaultEcs;
using PataNext.Game.Modules.Simulation.Application;
using revghost;
using revghost.Module;
using revghost.Utility;
using revghost.Injection.Dependencies;
using revghost.Threading.Components;
using revghost.Threading.V2;

namespace PataNext.Game.Modules.Simulation;

public partial class Module : HostModule
{
    private readonly HostRunnerScope _scope;

    private World _world;

    public Module(HostRunnerScope scope) : base(scope)
    {
        _scope = scope;

        Dependencies.AddRef(() => ref _world);
    }

    protected override void OnInit()
    {
        // Create the simulation domain
        {
            var listenerCollection = _world.CreateEntity();
            listenerCollection.Set<ListenerCollectionBase>(new ListenerCollection());

            var domain = _world.CreateEntity();
            domain.Set<IListener>(new SimulationDomain(_scope, domain));
            domain.Set(new PushToListenerCollection(listenerCollection));
            
            _world.Set(new CurrentSimulationClient(domain));
        }

        LoadModule(scope => new RhythmEngine.Module(scope));

        TrackDomain((SimulationDomain domain) =>
        {
            Disposables.AddRange(new IDisposable[]
            {
                new RunSystemGroupSystem(domain.Scope)
            });
        });
    }
}