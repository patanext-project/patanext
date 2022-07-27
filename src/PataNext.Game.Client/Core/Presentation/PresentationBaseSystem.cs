using System.Runtime.CompilerServices;
using Collections.Pooled;
using revecs.Core;
using revecs.Core.Components.Boards;
using revecs.Querying;
using revecs.Utility;
using revghost;
using revghost.Ecs;
using revghost.Injection.Dependencies;
using revghost.Shared;
using revghost.Utility;

namespace PataNext.Export.Godot.Presentation;

public abstract class PresentationBaseSystem : AppSystem
{
    private struct OnEntityRemoved
    {
        public class Board : SparseSetManagedComponentBoard<List<List<UEntitySafe>>>
        {
            public Board(int size, RevolutionWorld world) : base(size, world)
            {
            }
            
            public override void RemoveComponent(UEntityHandle handle)
            {
                var row = EntityLink[handle.Id];
                if (ComponentDataColumn[row.Id] != null!)
                {
                    var list = ComponentDataColumn[row.Id];
                    foreach (var nestedList in list)
                        nestedList.Add(World.Safe(handle));
                }
                
                base.RemoveComponent(handle);
            }
        }

        public static ComponentType<List<List<UEntitySafe>>> GetComponentType(RevolutionWorld world)
        {
            var name = ManagedTypeData<OnEntityRemoved>.Name;

            ComponentType type;
            if ((type = world.GetComponentType(name)).Equals(default))
            {
                type = world.RegisterComponent(name, new Board(Unsafe.SizeOf<OnEntityRemoved>(), world));
            }

            return type.UnsafeCast<List<List<UEntitySafe>>>();
        }
    }

    private readonly List<UEntitySafe> addList = new();
    private readonly List<UEntitySafe> removeList = new();
    private RevolutionWorld gameWorld;

    private IPresentationLoop updateLoop;

    public PresentationBaseSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref gameWorld);
        Dependencies.Add(() => ref updateLoop);
    }

    protected string SystemTypeName => GetType().FullName;

    protected ArchetypeQuery QueryAll { get; private set; }
    protected ArchetypeQuery QueryWithoutPresentation { get; private set; }
    protected ArchetypeQuery QueryWithPresentation { get; private set; }

    public RevolutionWorld GameWorld => gameWorld;
    public ComponentType ComponentType { get; private set; }

    private ComponentType<List<List<UEntitySafe>>> reactiveDestroyType;

    protected override void OnInit()
    {
        ComponentType = CreateComponentType();

        reactiveDestroyType = OnEntityRemoved.GetComponentType(GameWorld);

        using var all = new PooledList<ComponentType>();
        using var or = new PooledList<ComponentType>();
        using var none = new PooledList<ComponentType>();

        GetMatchedComponents(all, or, none);

        QueryAll = new ArchetypeQuery(GameWorld, all.Span, none.Span, or.Span);

        none.Add(ComponentType);
        QueryWithoutPresentation = new ArchetypeQuery(GameWorld, all.Span, none.Span, or.Span);

        none.RemoveAt(none.Count - 1);
        all.Add(ComponentType);
        QueryWithPresentation = new ArchetypeQuery(GameWorld, all.Span, none.Span, or.Span);

        Disposables.AddRange(new[]
        {
            QueryAll.IntendedBox(),
            QueryWithoutPresentation.IntendedBox(),
            QueryWithPresentation.IntendedBox(),
            updateLoop.Subscribe(OnPresentationLoop).IntendedBox()
        });
    }

    protected virtual ComponentType CreateComponentType()
    {
        ComponentType systemType;
        if ((systemType = GameWorld.GetComponentType(SystemTypeName)).Equals(default))
            systemType = GameWorld.RegisterComponent(SystemTypeName, new TagComponentBoard(GameWorld));

        return systemType;
    }

    protected abstract void GetMatchedComponents(
        PooledList<ComponentType> all,
        PooledList<ComponentType> or,
        PooledList<ComponentType> none);

    protected abstract bool EntityMatch(in UEntityHandle entity);

    protected abstract void OnSetPresentation(in UEntitySafe entity);
    protected abstract void OnRemovePresentation(in UEntitySafe entity);

    protected virtual void OnPresentationLoop()
    {
        foreach (ref readonly var handle in QueryWithoutPresentation)
        {
            if (EntityMatch(in handle) == false)
                continue;

            addList.Add(gameWorld.Safe(handle));
        }

        foreach (ref readonly var handle in QueryWithPresentation)
        {
            if (EntityMatch(in handle))
                continue;

            removeList.Add(gameWorld.Safe(handle));
        }

        for (var index = 0; index < addList.Count; index++)
        {
            var entity = addList[index];
            var handle = entity.Handle;
            
            OnSetPresentation(entity);
            if (GameWorld.HasComponent(handle, ComponentType))
            {
                if (!GameWorld.HasComponent(handle, reactiveDestroyType))
                {
                    GameWorld.AddComponent(handle, reactiveDestroyType, default);
                    GameWorld.GetComponentData(handle, reactiveDestroyType) = new List<List<UEntitySafe>>();
                }

                GameWorld.GetComponentData(handle, reactiveDestroyType).Add(removeList);
            }

            addList.RemoveAt(index--);
        }

        for (var index = 0; index < removeList.Count; index++)
        {
            var handle = removeList[index];
            OnRemovePresentation(handle);

            removeList.RemoveAt(index--);
        }
    }
}