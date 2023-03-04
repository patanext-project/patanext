using System.Runtime.InteropServices;
using Godot;
using PataNext.GodotServer;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using revghost;
using revghost.Module;

namespace PataNext;

public class TestServerModule : HostModule
{
    public TestServerModule(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        LoadModule(sc => new PataNext.Game.Client.Resources.Module(sc));
        LoadModule(sc => new PataNext.Game.Module(sc));
        LoadModule(sc => new PataNext.Game.Client.Module(sc));

        LoadModule(sc => new GodotServer.Module(sc));
        
        TrackDomain((SimulationDomain domain) =>
        {
            var world = domain.World;
            var simulation = domain.GameWorld;
            var mesh = world.CreateEntity();
            {
                var boxMesh = new BoxMesh();
                boxMesh.Size = new Vector3(1, 2, 1);

                // needed or else the finalizer will get called and will destroy the object even though it still exist
                GCHandle.Alloc(boxMesh, GCHandleType.Normal);

                mesh.Set(boxMesh.GetRid());
            }

            for (var i = 0; i < 10_000; i++)
            {
                var ent = simulation.CreateEntity();
                simulation.AddPositionComponent(ent, new(i, 0));
                simulation.AddRotationComponent(ent, new(0));
                simulation.AddRenderMesh(ent, new() {Mesh = mesh});
            }
        });
    }
}