using DefaultEcs;
using revecs.Systems;
using revghost;
using revghost.Domains.Time;
using revghost.Ecs;
using revghost.Injection.Dependencies;
using revghost.Loop.EventSubscriber;
using revghost.Utility;
using revtask.Core;

namespace PataNext.Game.Modules.Simulation.Application;

public class RunSystemGroupSystem : AppSystem
{
    private IDomainUpdateLoopSubscriber _updateLoop;
    private SystemGroup _systemGroup;
    private IJobRunner _jobRunner;

    public JobRequest Job;
    public Entity Start, End;

    public RunSystemGroupSystem(Scope scope) : base(scope)
    {
        scope.Context.Register(this);

        Dependencies.AddRef(() => ref _updateLoop);
        Dependencies.AddRef(() => ref _systemGroup);
        Dependencies.AddRef(() => ref _jobRunner);
    }

    protected override void OnInit()
    {
        Disposables.AddRange(new IDisposable[]
        {
            Start = _updateLoop.Subscribe(OnUpdate),
            End = _updateLoop.Subscribe(OnUpdateEnd, builder =>
            {
                builder
                    .After(Start)
                    .Position(OrderPosition.AtEnd);
            }),
        });
    }

    private void OnUpdate(WorldTime obj)
    {
        Job = _systemGroup.Schedule(_jobRunner);
    }

    private void OnUpdateEnd(WorldTime obj)
    {
        _jobRunner.CompleteBatch(Job);
    }
}