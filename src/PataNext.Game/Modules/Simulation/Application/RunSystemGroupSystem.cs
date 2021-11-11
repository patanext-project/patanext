using Collections.Pooled;
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

public interface ISimulationParallelUpdateLoopSubscriber : IEventSubscriber
{
    Entity Subscribe(Action<JobRequest> callback, ProcessOrder process = null);
}

public class SimulationParallelUpdateLoop : ISimulationParallelUpdateLoopSubscriber,
    IDisposable
{
    private readonly OrderGroup _orderGroup;
    private Entity _callbackEntity;
    private readonly PooledList<Action<JobRequest>> _callbacks = new(ClearMode.Always);

    public SimulationParallelUpdateLoop(World world)
    {
        _orderGroup = new OrderGroup();
        _callbackEntity = world.CreateEntity();
    }

    public void Dispose()
    {
        _orderGroup.Dispose();
        _callbackEntity.Dispose();
    }

    public Entity Subscribe(Action<Entity> callback, ProcessOrder process)
    {
        return Subscribe((JobRequest _) => callback(_callbackEntity), process);
    }

    public Entity Subscribe(Action<JobRequest> callback, ProcessOrder process)
    {
        var entity = _orderGroup.Add(process);
        entity.Set(in callback);
        return entity;
    }

    public void Invoke(JobRequest systemJob)
    {
        if (_orderGroup.Build())
        {
            _callbacks.ClearReference();
            var entities = _orderGroup.Entities;
            for (var index = 0; index < entities.Length; ++index)
                _callbacks.Add(entities[index].Get<Action<JobRequest>>());
        }
        
        _callbackEntity.Set(systemJob);

        foreach (var action in _callbacks.Span)
            action(systemJob);
    }
}

public class RunSystemGroupSystem : AppSystem
{
    private readonly Scope scope;

    private IDomainUpdateLoopSubscriber updateLoop;
    private SystemGroup systemGroup;
    private IJobRunner jobRunner;
    private World world;

    private SimulationParallelUpdateLoop simulationUpdateLoop;

    public RunSystemGroupSystem(Scope scope) : base(scope)
    {
        scope.Context.Register(this);
        
        this.scope = scope;

        Dependencies.AddRef(() => ref updateLoop);
        Dependencies.AddRef(() => ref systemGroup);
        Dependencies.AddRef(() => ref jobRunner);
        Dependencies.AddRef(() => ref world);
    }

    public Entity UpdateLoopEntity { get; private set; }

    protected override void OnInit()
    {
        simulationUpdateLoop = new SimulationParallelUpdateLoop(world);
        scope.Context.Register<ISimulationParallelUpdateLoopSubscriber>(simulationUpdateLoop);

        Disposables.AddRange(new IDisposable[]
        {
            simulationUpdateLoop,
            UpdateLoopEntity = updateLoop.Subscribe(OnUpdate)
        });
    }

    private void OnUpdate(WorldTime obj)
    {
        var systemJob = systemGroup.Schedule(jobRunner);
        {
            simulationUpdateLoop.Invoke(systemJob);
        }
        jobRunner.CompleteBatch(systemJob);
    }
}