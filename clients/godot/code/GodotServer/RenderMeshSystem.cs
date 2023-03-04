using System.Numerics;
using DefaultEcs;
using Godot;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using revecs;
using revghost;
using revghost.Utility;
using Quaternion = Godot.Quaternion;

namespace PataNext.GodotServer;

public partial class RenderMeshSystem : SimulationSystem
{
    public RenderMeshSystem(Scope scope) : base(scope)
    {
        SubscribeTo<IPresentationLoop>(OnUpdate);
    }

    private UnitializedMeshQuery _uninitQuery;
    private MeshQuery _query;

    protected override void OnInit()
    {
        _uninitQuery = new UnitializedMeshQuery(Simulation);
        _query = new MeshQuery(Simulation);
    }
    
    private void OnUpdate(Entity obj)
    {
        var root = (Node3D) ((SceneTree) Engine.GetMainLoop()).CurrentScene;

        foreach (var ent in _uninitQuery)
        {
            var mesh = ent.mesh.Mesh;
            var meshRID = mesh.Get<RID>();

            var simuId = RenderingServer.InstanceCreate();

            Simulation.AddGodotInstanceId(ent.Handle, new GodotInstanceId(
                simuId
            ));
            Simulation.AddGodotInstanceMeshId(ent.Handle, new GodotInstanceMeshId(
                meshRID
            ));
            RenderingServer.InstanceSetBase(simuId, meshRID);
            RenderingServer.InstanceSetScenario(simuId, root.GetWorld3d().Scenario);
        }

        if (done)
            return;

        // done = true;

        _query.QueueAndComplete(Runner, static (state, iteration) =>
        {
            foreach (var ent in iteration)
            {
                var rid = ent.id.Value;
            
                RenderingServer.InstanceSetTransform(rid, new Transform3D(
                    Quaternion.Identity,
                    MathUtils.SwapNumericsToGodot(new System.Numerics.Vector3(ent.pos.Value, 0))
                ));
                RenderingServer.InstanceSetVisible(rid, ent.pos.X < 10);
            }
        });
    }

    private bool done = false;

    public partial record struct UnitializedMeshQuery : IQuery<(
        Read<RenderMesh> mesh,
        Read<PositionComponent> pos,
        Read<RotationComponent> rot,
        None<GodotInstanceMeshId>)>;
    
    public partial record struct MeshQuery : IQuery<(
        Read<RenderMesh> mesh,
        Write<PositionComponent> pos,
        Read<RotationComponent> rot,
        Read<GodotInstanceMeshId> meshId,
        Read<GodotInstanceId> id)>;
}