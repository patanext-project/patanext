using System.Runtime.CompilerServices;
using GodotCLR;
using revecs.Core;
using revecs.Core.Components.Boards;
using revghost;

using NodeProxy = GodotCLR.GD.Node;

namespace PataNext.Export.Godot.Presentation;

public abstract class PresentationGodotBaseSystem : PresentationBaseSystem
{
    protected PresentationGodotBaseSystem(Scope scope) : base(scope)
    {
    }

    private Dictionary<UEntitySafe, NodeProxy> entitiesToProxies = new();

    protected ComponentType<NodeProxy> GenericType;

    protected override ComponentType CreateComponentType()
    {
        ComponentType systemType;
        if ((systemType = GameWorld.GetComponentType(SystemTypeName)).Equals(default))
        {
            systemType = GameWorld.RegisterComponent(
                SystemTypeName,
                new SparseSetComponentBoard(Unsafe.SizeOf<NodeProxy>(), GameWorld)
            );
        }

        return GenericType = GameWorld.AsGenericComponentType<NodeProxy>(systemType);
    }

    protected override void OnSetPresentation(in UEntitySafe entity)
    {
        if (!OnSetPresentation(entity, out var proxy))
        {
            if (proxy.Pointer == IntPtr.Zero)
                throw new InvalidOperationException("proxy shouldn't have been created if it returned false");
            
            return;
        }
        
        entitiesToProxies.Add(entity, proxy);

        GameWorld.AddComponent(entity.Handle, GenericType, proxy);
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

    protected abstract bool OnSetPresentation(in UEntitySafe entity, out NodeProxy node);

    protected abstract bool OnRemovePresentation(in UEntitySafe entity, in NodeProxy node);
}