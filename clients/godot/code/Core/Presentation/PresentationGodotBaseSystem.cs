using System.Runtime.CompilerServices;
using Godot;
using revecs.Core;
using revecs.Core.Components.Boards;
using revecs.Systems;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Shared.Collections.Concurrent;
using revghost.Threading.V2.Apps;
using revtask.Core;


namespace PataNext.Export.Godot.Presentation;

// async version!

public abstract class PresentationGodotBaseSystem : PresentationBaseSystem
{
    protected IJobRunner runner;

    private IReadOnlyDomainWorker _worker;
    
    protected PresentationGodotBaseSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref runner!);
        Dependencies.Add(() => ref _worker!);
    }

    private Dictionary<UEntitySafe, Node> entitiesToProxies = new();
    private ConcurrentList<(UEntitySafe entity, Node result)> jobQueue = new();

    protected ComponentType<EntityData> GenericType;

    protected override ComponentType CreateComponentType()
    {
        ComponentType systemType;
        if ((systemType = GameWorld.GetComponentType(SystemTypeName)).Equals(default))
        {
            systemType = GameWorld.RegisterComponent(
                SystemTypeName,
                new SparseSetManagedComponentBoard<EntityData>(Unsafe.SizeOf<EntityData>(), GameWorld)
            );
        }

        return GenericType = GameWorld.AsGenericComponentType<EntityData>(systemType);
    }

    protected override void OnSetPresentation(in UEntitySafe entity)
    {
        if (!OnSetPresentation(entity, out var proxy))
        {
            if (proxy != null)
                throw new InvalidOperationException("proxy shouldn't have been created if it returned false");
            
            return;
        }
        
        GameWorld.AddComponent(entity.Handle, GenericType, new EntityData
        {
            Node = proxy
        });
        
        jobQueue.Add((entity, proxy));
    }

    protected override void OnRemovePresentation(in UEntitySafe entity)
    {
        var node = entitiesToProxies[entity];

        if (!OnRemovePresentation(entity, node))
            return;

        node.QueueFree();
        entitiesToProxies.Remove(entity);

        if (GameWorld.Exists(entity))
        {
            // ? it was here
            //GameWorld.GetComponentData(entity.Handle, GenericType).Dispose();
            GameWorld.RemoveComponent(entity.Handle, GenericType);
        }
    }

    protected abstract bool OnSetPresentation(in UEntitySafe entity, out Node proxy);

    protected abstract bool OnRemovePresentation(in UEntitySafe entity, in Node node);

    protected bool TryGetNode(UEntityHandle handle, out Node node)
    {
        node = GameWorld.GetComponentData(handle, GenericType).Node;
        return node != null;
    }

    private int r = 0;
    protected override void OnPresentationLoop()
    {
        var root = ((SceneTree) Engine.GetMainLoop()).CurrentScene;
        
        while (jobQueue.Count > 0)
        {
            var (entity, result) = jobQueue[0];
            jobQueue.RemoveAt(0);

            Console.WriteLine("found " + result);
            if (result == null)
                throw new NullReferenceException(nameof(result));
            
            entitiesToProxies.Add(entity, result);
            GameWorld.GetComponentData(entity.Handle, GenericType).Node = result;
            
            root.AddChild(result);
            Console.WriteLine("added to " + root);
        }
        
        base.OnPresentationLoop();
    }

    protected struct EntityData
    {
        public Node Node;
    }
}