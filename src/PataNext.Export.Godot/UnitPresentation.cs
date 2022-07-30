using System.Numerics;
using Collections.Pooled;
using GodotCLR;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Units;
using revecs.Core;
using revghost;
using revtask.Core;

namespace PataNext.Export.Godot;

public class UnitPresentation : PresentationGodotBaseSystem
{
    private GD.PackedScene packedScene;
    
    public UnitPresentation(Scope scope) : base(scope)
    {
        packedScene = GD.ResourceLoader.Load("res://scenes/models/uberhero/Uberhero.tscn")
            .To<GD.PackedScene>();
    }

    protected override void GetMatchedComponents(PooledList<ComponentType> all, PooledList<ComponentType> or, PooledList<ComponentType> none)
    {
        all.Add(UnitDescription.ToComponentType(GameWorld));
        all.Add(PositionComponent.ToComponentType(GameWorld));
    }

    protected override bool EntityMatch(in UEntityHandle entity)
    {
        return true;
    }

    protected override bool OnSetPresentation(in UEntitySafe entity, out JobRequest job)
    {
        job = NewInstantiateJob(entity, packedScene, false);
        return true;
    }

    protected override bool OnRemovePresentation(in UEntitySafe entity, in GD.Node node)
    {
        return true;
    }
    
    protected override void OnPresentationLoop()
    {
        base.OnPresentationLoop();

        var posAccessor = GameWorld.AccessSparseSet(PositionComponent.Type.GetOrCreate(GameWorld));
        foreach (var entity in QueryWithPresentation)
        {
            if (!TryGetNode(entity, out var node))
                continue;

            /*node.SetProperty("position", new Variant
            {
                Type = Variant.EType.VECTOR3,
                Vector3 = new Vector3(posAccessor[entity].Value, 0)
            });*/
            //Console.WriteLine($"{posAccessor[entity].Value}");
            node.To<GD.Node3D>()
                .SetPosition(new Vector3(posAccessor[entity].Value, 0));
        }
    }
}