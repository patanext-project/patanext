using DefaultEcs;
using Quadrum.Game.Modules.Client.Audio;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Utilities;
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

    public UpdatePresentationSystems(Scope scope) : base(scope)
    {
        this.scope = scope;

        Dependencies.Add(() => ref world);
        Dependencies.Add(() => ref domainUpdate);
    }

    private PresentationLoop loop;

    protected override void OnInit()
    {
        loop = new PresentationLoop(world);

        scope.Context.Register<IPresentationLoop>(loop);

        Disposables.AddRange(new IDisposable[]
        {
            loop,
            domainUpdate.Subscribe(
                OnUpdate,
                b => b
                    .BeforeGroup<AudioSystemGroup>()
            )
        });
    }

    private void OnUpdate(WorldTime time)
    {
        loop.Invoke();
    }
}