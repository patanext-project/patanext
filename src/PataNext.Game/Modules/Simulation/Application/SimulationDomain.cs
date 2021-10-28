using System.Diagnostics;
using DefaultEcs;
using revecs.Core;
using revecs.Systems;
using revghost;
using revghost.Domains;
using revghost.Domains.Time;
using revghost.Injection;
using revghost.Loop;
using revghost.Loop.EventSubscriber;
using revghost.Shared.Threading.Schedulers;
using revghost.Threading.V2;
using revghost.Threading.V2.Apps;
using revtask.Core;
using revtask.OpportunistJobRunner;

namespace PataNext.Game.Modules.Simulation.Application;

// TODO: Move it into revecs.revghostExtension project
public class SimulationDomain : CommonDomainThreadListener
{
    public readonly SimulationScope Scope;

    public readonly World World;
    public readonly RevolutionWorld GameWorld;
    public readonly SystemGroup SystemGroup;
    public readonly IJobRunner JobRunner;

    private readonly OpportunistJobRunner _jobRunner;

    public readonly IManagedWorldTime WorldTime;
    private readonly ManagedWorldTime _worldTime;

    public readonly IDomainUpdateLoopSubscriber UpdateLoop;
    private readonly DefaultDomainUpdateLoopSubscriber _updateLoop;

    private readonly Stopwatch _sleepTime = new();
    private readonly DomainWorker _worker;

    public TimeSpan? TargetFrequency
    {
        get => _targetFrequency;
        set => Scheduler.Add(args => args.t._targetFrequency = args.v, (t: this, v: value));
    }

    private TimeSpan? _targetFrequency;
    private FixedTimeStep _fts;

    private int _currentFrame;

    private UEntityHandle _timeEntity;

    public SimulationDomain(Scope scope, Entity domainEntity) : base(scope, domainEntity)
    {
        Scope = new SimulationScope(DomainScope);
        {
            World = Scope.World;
            GameWorld = Scope.GameWorld;
            SystemGroup = Scope.SystemGroup;
            JobRunner = Scope.JobRunner;

            _jobRunner = (OpportunistJobRunner) JobRunner;

            Scope.Context.Register(WorldTime = _worldTime = new ManagedWorldTime());
            Scope.Context.Register(UpdateLoop = _updateLoop = new DefaultDomainUpdateLoopSubscriber(World));
        }

        _targetFrequency = TimeSpan.FromMilliseconds(10);

        if (!scope.Context.TryGet(out _worker))
            _worker = new DomainWorker("Simulation Domain");

        _timeEntity = GameWorld.CreateEntity();

        GameWorld.AddComponent(_timeEntity, GameTime.Type.GetOrCreate(GameWorld), default);
    }

    protected override void DomainUpdate()
    {
        // future proof for a rollback system
        _worldTime.Total = _currentFrame * _worldTime.Delta;
        {
            GameWorld.GetComponentData(_timeEntity, GameTime.Type.GetOrCreate(GameWorld)) = new GameTime
            {
                Frame = _currentFrame,
                Total = _worldTime.Total,
                Delta = _worldTime.Delta
            };
            
            _updateLoop.Invoke(_worldTime.Total, _worldTime.Delta);
        }
    }

    protected override ListenerUpdate OnUpdate()
    {
        if (IsDisposed || _disposalStartTask.Task.IsCompleted)
            return default;

        var delta = _worker.Delta + _sleepTime.Elapsed;

        var updateCount = 1;
        if (_targetFrequency is { } targetFrequency)
        {
            _fts.SetTargetFrameTime(targetFrequency);
            
            updateCount = Math.Clamp(_fts.GetUpdateCount(delta.TotalSeconds), 0, 3);
        }
        
        // If we don't have a target frequency, use the delta
        using (_worker.StartMonitoring(_targetFrequency ?? delta))
        {
            _worldTime.Delta = _targetFrequency ?? delta;

            using (CurrentUpdater.SynchronizeThread())
            {
                Scheduler.Run();
                TryExecuteScheduler();

                try
                {
                    _jobRunner.StartPerformanceCriticalSection();
                    for (var tickAge = updateCount - 1; tickAge >= 0; --tickAge)
                    {
                        _currentFrame++;
                        DomainUpdate();
                    }
                }
                finally
                {
                    _jobRunner.StopPerformanceCriticalSection();
                }
            }
        }
        
        var timeToSleep =
            TimeSpan.FromTicks(
                Math.Max(
                    (_targetFrequency ?? TimeSpan.FromMilliseconds(1)).Ticks - _worker.Delta.Ticks,
                    0
                )
            );

        _sleepTime.Restart();
        return new ListenerUpdate
        {
            TimeToSleep = timeToSleep
        };
    }
}