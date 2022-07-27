using System.Numerics;
using PataNext.Export.Godot.Presentation;
using PataNext.Game.Client.Core.Inputs;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.Units;
using revecs;
using revecs.Core;
using revecs.Extensions.Generator.Commands;
using revecs.Extensions.Generator.Components;
using revecs.Extensions.RelativeEntity;
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
        // TODO: depend on resource module
        LoadModule(sc => new PataNext.Game.Client.Resources.Module(sc));
        LoadModule(sc => new PataNext.Game.Module(sc));
        LoadModule(sc => new PataNext.Game.Client.Module(sc));

        TrackDomain((SimulationDomain domain) =>
        {
            new RhythmEnginePresentation(domain.Scope);
            new UnitPresentation(domain.Scope);

            var size = 0;
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    var unit = domain.GameWorld.CreateEntity();
                    domain.GameWorld.AddComponent(unit, UnitDescription.Type.GetOrCreate(domain.GameWorld), default);
                    domain.GameWorld.AddPositionComponent(unit, new(
                        (x - size / 2) * 1.5f,
                        (y - size / 2) * 1.7f
                    ));
                    domain.GameWorld.GetPositionComponent(unit).Value = new Vector2(
                        (x - size / 2) * 1.5f,
                        (y - size / 2) * 1.7f
                    );
                    Console.WriteLine($"{x} {y} -> {(x - size / 2) * 1.5f}, {(y - size / 2) * 1.7f}");
                }
            }

            // for now create a random player entity
            var player = domain.GameWorld.CreateEntity();
            domain.GameWorld.AddComponent(player, PlayerDescription.Type.GetOrCreate(domain.GameWorld), default);
            domain.GameWorld.AddComponent(player, GameRhythmInput.Type.GetOrCreate(domain.GameWorld), default);
            
            // and a random rhythm engine
            var engine = domain.GameWorld.CreateEntity();
            domain.GameWorld.AddComponent(engine, RhythmEngineLayout.Type.GetOrCreate(domain.GameWorld), default);

            ref var controller = ref domain.GameWorld.GetComponentData(
                engine,
                RhythmEngineController.Type.GetOrCreate(domain.GameWorld)
            );
            
            controller = controller with
            {
                State = RhythmEngineController.EState.Playing,
                StartTime = domain.WorldTime.Total.Add(TimeSpan.FromSeconds(2))
            };

            ref var settings = ref domain.GameWorld.GetComponentData(
                engine,
                RhythmEngineSettings.Type.GetOrCreate(domain.GameWorld)
            );

            settings = settings with
            {
                MaxBeats = 4,
                BeatInterval = TimeSpan.FromMilliseconds(500)
            };

            ref var comboSettings = ref domain.GameWorld.GetComponentData(
                engine,
                GameComboSettings.Type.GetOrCreate(domain.GameWorld)
            );

            comboSettings = comboSettings with
            {
                MaxComboToReachFever = 9,
                RequiredScoreStart = 4,
                RequiredScoreStep = 0.5f
            };

            domain.GameWorld.AddComponent(
                engine,
                PlayerDescription.Relative.Type.GetOrCreate(domain.GameWorld),
                player
            );



            /*var archetype = domain.GameWorld.GetArchetype(engine);
            var components = domain.GameWorld.ArchetypeBoard.GetComponentTypes(archetype);
            foreach (var comp in components)
            {
                Console.WriteLine($"{comp.Handle} <-- {domain.GameWorld.ComponentTypeBoard.Names[comp.Handle]}");
            }*/
        });
            
    }
}