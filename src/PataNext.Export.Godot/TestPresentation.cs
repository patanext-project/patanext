using Collections.Pooled;
using PataNext.Export.Godot.Presentation;
using PataNext.Game.Modules.Simulation.Common.Transform;
using revecs.Core;
using revghost;
using RustTest;

namespace PataNext.Export.Godot;

public class TestPresentation : PresentationGodotBaseSystem
{
    public TestPresentation(Scope scope) : base(scope)
    {
    }

    protected override void GetMatchedComponents(
        PooledList<ComponentType> all,
        PooledList<ComponentType> or,
        PooledList<ComponentType> none)
    {
        all.Add(PositionComponent.Type.GetOrCreate(GameWorld));
        all.Add(VelocityComponent.Type.GetOrCreate(GameWorld));
    }

    protected override bool EntityMatch(in UEntityHandle entity)
    {
        return true;
    }

    protected override bool OnSetPresentation(in UEntitySafe entity, out NodeProxy node)
    {
        node = new NodeProxy($"proxy {entity.Row} : {entity.Version}", "res://my_node.tscn");
        return true;
    }

    protected override bool OnRemovePresentation(in UEntitySafe entity, in NodeProxy node)
    {
        //Console.WriteLine("remove " + entity.Handle);
        return true;
    }

    protected override void OnPresentationLoop()
    {
        base.OnPresentationLoop();

        foreach (var entity in QueryWithPresentation)
        {
            var node = GameWorld.GetComponentData(entity, GenericType);
            node.SetPosition2D(GameWorld.GetComponentData(entity, PositionComponent.Type.GetOrCreate(GameWorld)).Value);
        }
    }
}