using DefaultEcs;
using PataNext.Game.Modules.Simulation.Application;
using revghost;
using revghost.Domains.Time;
using revghost.Ecs;
using revghost.Injection.Dependencies;
using revghost.Loop.EventSubscriber;
using revghost.Utility;

namespace PataNext.Export.Godot.Presentation;

public class UpdatePresentationSystems : AppSystem
{
    private readonly Scope scope;

    private World world;
    private IDomainUpdateLoopSubscriber domainUpdate;
    private RunSystemGroupSystem simulationSystemGroup;

    public UpdatePresentationSystems(Scope scope) : base(scope)
    {
        this.scope = scope;

        Dependencies.AddRef(() => ref world);
        Dependencies.AddRef(() => ref domainUpdate);
        Dependencies.AddRef(() => ref simulationSystemGroup);
    }

    private PresentationLoop loop;

    protected override void OnInit()
    {
        loop = new PresentationLoop(world);

        scope.Context.Register<IPresentationLoop>(loop);

        Disposables.AddRange(new IDisposable[]
        {
            loop,
            domainUpdate.Subscribe(OnUpdate, b => { b.After(simulationSystemGroup.UpdateLoopEntity); })
        });
    }

    private void OnUpdate(WorldTime time)
    {
        loop.Invoke();
    }
}