using Collections.Pooled;
using Godot;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Units;
using revecs.Core;
using revghost;

namespace PataNext.Presentations;

public partial class TestUnitPresentation : Node3D
{
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        RotateY((float) delta * 2.0f);
    }
}

public class TestUnitPresentationSystem : PresentationGodotBaseSystem
{
    private readonly PackedScene _scene = ResourceLoader.Load<PackedScene>("res://models/units/uberhero_base.tscn");

    public TestUnitPresentationSystem(Scope scope) : base(scope)
    {
    }

    protected override void GetMatchedComponents(PooledList<ComponentType> all, PooledList<ComponentType> or,
        PooledList<ComponentType> none)
    {
        all.Add(UnitDescription.ToComponentType(GameWorld));
        all.Add(PlayableUnitLayout.ToComponentType(GameWorld));
    }

    protected override bool EntityMatch(in UEntityHandle entity)
    {
        return true;
    }

    protected override bool OnSetPresentation(in UEntitySafe entity, out Node proxy)
    {
        proxy = _scene.Instantiate().Duplicate();
        return true;
    }

    protected override bool OnRemovePresentation(in UEntitySafe entity, in Node node)
    {
        node.Dispose();
        return true;
    }

    protected override void OnPresentationLoop()
    {
        base.OnPresentationLoop();

        foreach (var entity in QueryWithPresentation)
        {
            if (!TryGetNode(entity, out var node))
                continue;

            var pos = GameWorld.GetPositionComponent(entity);

            var unit = (TestUnitPresentation) node;
            unit.Position = new Vector3(
                pos.X,
                pos.Y,
                0
            );
        }
    }
}