using System.Numerics;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using revecs;
using revecs.Extensions.Generator.Commands;
using revecs.Extensions.Generator.Components;
using revecs.Systems;
using revghost;
using revghost.Module;

namespace PataNext.Export.Godot;

public partial class EntryModule : HostModule
{
    public EntryModule(HostRunnerScope scope) : base(scope)
    {
        Console.WriteLine("EntryModule - .ctor");
    }

    protected override void OnInit()
    {
        // Add Godot related modules here

        Console.WriteLine("EntryModule - OnInit");
        LoadModule(scope => new PataNext.Game.Module(scope));

        TrackDomain((SimulationDomain domain) =>
        {
            domain.SystemGroup.Add(CreateUnit);
            domain.SystemGroup.Add(MoveUnit);

            new UpdatePresentationSystems(domain.Scope);
            new TestPresentation(domain.Scope);
        });
    }

    [RevolutionSystem]
    private static void CreateUnit(
        [Query, Optional] limit<With<PositionComponent>> query,
        [Cmd] c<
        ICmdEntityAdmin,
        PositionComponent.Cmd.IAdmin,
        VelocityComponent.Cmd.IAdmin,
        BounceCount.Cmd.IAdmin> cmd)
    {
        if (query.GetEntityCount() > 100)
            return;
        
        var random = Random.Shared;
        
        var ent = cmd.CreateEntity();
        cmd.AddPositionComponent(ent,
            new PositionComponent(
                random.Next(0, 100),
                random.Next(0, 100)
            )
        );
        cmd.AddVelocityComponent(ent,
            new VelocityComponent(
                Vector2.Normalize(new Vector2(
                    random.Next(-100, 100),
                    random.Next(-100, 100)
                )) * 1000
            )
        );
        cmd.AddBounceCount(ent);
    }

    [RevolutionSystem]
    [DependOn(nameof(CreateUnit))]
    private static void MoveUnit(
        [Query] GameTimeSingleton time,
        [Query] q<Write<PositionComponent>, Write<VelocityComponent>, Write<BounceCount>> movables,
        [Cmd] mov_c<ICmdEntityAdmin> cmd)
    {
        var dt = (float) time.Delta.TotalSeconds;
        foreach (var (handle, pos, vel, bounce) in movables)
        {
            pos.Value += vel.Value * dt;
            if (pos.Value.X < 0)
            {
                pos.Value.X = 0;
                vel.Value.X = -vel.Value.X;

                bounce.Value++;
            }

            if (pos.Value.X > 1000)
            {
                pos.Value.X = 1000;
                vel.Value.X = -vel.Value.X;

                bounce.Value++;
            }

            if (pos.Value.Y < 0)
            {
                pos.Value.Y = 0;
                vel.Value.Y = -vel.Value.Y;

                bounce.Value++;
            }

            if (pos.Value.Y > 1000)
            {
                pos.Value.Y = 1000;
                vel.Value.Y = -vel.Value.Y;

                bounce.Value++;
            }

            if (bounce.Value >= 3)
                cmd.DestroyEntity(handle);
        }
    }

    public partial struct BounceCount : ISparseComponent
    {
        public int Value;
    }
}