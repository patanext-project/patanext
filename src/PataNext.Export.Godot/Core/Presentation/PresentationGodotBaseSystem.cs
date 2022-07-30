using System.Runtime.CompilerServices;
using GodotCLR;
using revecs.Core;
using revecs.Core.Components.Boards;
using revecs.Extensions.Generator.Components;
using revecs.Systems;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Shared;
using revghost.Shared.Collections.Concurrent;
using revtask.Core;
using revtask.Helpers;
using NodeProxy = GodotCLR.GD.Node;

namespace PataNext.Export.Godot.Presentation;

// async version!

public abstract class PresentationGodotBaseSystem : PresentationBaseSystem
{
    protected IJobRunner runner;
    
    protected PresentationGodotBaseSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref runner!);
    }

    private Dictionary<UEntitySafe, NodeProxy> entitiesToProxies = new();
    private ConcurrentList<(UEntitySafe entity, NodeProxy result)> jobQueue = new();

    protected ComponentType<EntityData> GenericType;

    protected override ComponentType CreateComponentType()
    {
        ComponentType systemType;
        if ((systemType = GameWorld.GetComponentType(SystemTypeName)).Equals(default))
        {
            systemType = GameWorld.RegisterComponent(
                SystemTypeName,
                new SparseSetComponentBoard(Unsafe.SizeOf<EntityData>(), GameWorld)
            );
        }

        return GenericType = GameWorld.AsGenericComponentType<EntityData>(systemType);
    }

    protected override void OnSetPresentation(in UEntitySafe entity)
    {
        if (!OnSetPresentation(entity, out var job))
        {
            if (job.Version != 0)
                throw new InvalidOperationException("proxy shouldn't have been created if it returned false");
            
            return;
        }
        
        GameWorld.AddComponent(entity.Handle, GenericType, new EntityData
        {
            Request = job,
            Node = default
        });
        
        /*entitiesToProxies.Add(entity, proxy);

        GameWorld.AddComponent(entity.Handle, GenericType, proxy);*/
    }

    protected override void OnRemovePresentation(in UEntitySafe entity)
    {
        var node = entitiesToProxies[entity];

        if (!OnRemovePresentation(entity, node))
            return;

        node.Call("queue_free", ReadOnlySpan<Variant>.Empty);
        entitiesToProxies.Remove(entity);

        if (GameWorld.Exists(entity))
        {
            // ? it was here
            //GameWorld.GetComponentData(entity.Handle, GenericType).Dispose();
            GameWorld.RemoveComponent(entity.Handle, GenericType);
        }
    }

    protected abstract bool OnSetPresentation(in UEntitySafe entity, out JobRequest job);

    protected abstract bool OnRemovePresentation(in UEntitySafe entity, in NodeProxy node);

    protected bool TryGetNode(UEntityHandle handle, out NodeProxy node)
    {
        node = GameWorld.GetComponentData(handle, GenericType).Node;
        return node.Pointer != IntPtr.Zero;
    }

    private int r = 0;
    protected override void OnPresentationLoop()
    {
        var root = new GD.Node(GD.SceneTree.GetCurrentScene(GD.Engine.GetMainLoop()));
        
        while (jobQueue.Count > 0)
        {
            var (entity, result) = jobQueue[0];
            jobQueue.RemoveAt(0);

            if (result.Pointer == IntPtr.Zero)
                throw new NullReferenceException(nameof(result.Pointer));
            
            entitiesToProxies.Add(entity, result);
            GameWorld.GetComponentData(entity.Handle, GenericType).Node = result;
            
            root.AddChild(result);
        }
        
        base.OnPresentationLoop();
    }

    private JobRequest previousRequest;

    public JobRequest NewInstantiateJob(UEntitySafe caller, GD.PackedScene packedScene, bool duplicate = true)
    {
        new InstantiateJob
        {
            System = this,
            Caller = caller,
            Scene = packedScene,
            Duplicate = duplicate
        }.Execute(runner, default);
        return default;
        return runner.Queue(new InstantiateJob
        {
            System = this,
            Caller = caller,
            Scene = packedScene,
            Duplicate = duplicate
        });
        return default;
    }

    private static SwapDependency GodotThreadingDependency = new SwapDependency();
    
    private struct InstantiateJob : IJob, IJobExecuteOnCondition
    {
        public PresentationGodotBaseSystem System;
        public UEntitySafe Caller;
        public GD.PackedScene Scene;
        public bool Duplicate;

        public int SetupJob(JobSetupInfo info)
        {
            return 1;
        }

        public void Execute(IJobRunner runner, JobExecuteInfo info)
        {
            if (Duplicate)
                Scene = Scene.Duplicate();

            Scene.Reference();
            
            var node = Scene.Instantiate();
            System.jobQueue.Add((Caller, node));
        }

        public bool CanExecute(IJobRunner runner, JobExecuteInfo info)
        {
            using (SwapDependency.BeginContext())
            {
                return GodotThreadingDependency.TrySwap(runner, info.Request);
            }
        }
    }

    protected struct EntityData
    {
        public JobRequest Request;
        public NodeProxy Node;
    }
}
