using System.Numerics;
using GodotCLR;
using PataNext.Export.Godot.Presentation;
using PataNext.Game.Client.Core.Inputs;
using PataNext.Game.Modules.Abilities;
using PataNext.Game.Modules.RhythmEngine.Commands;
using PataNext.Module.Abilities.Providers.Defaults;
using PataNext.Module.Abilities.Scripts.Defaults;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Aspects;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Conditions;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
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
using revghost.Injection;
using revghost.Loop.EventSubscriber;
using revghost.Module;
using revghost.Shared.Threading.Tasks;
using revghost.Threading.V2.Apps;
using revghost.Utility;
using revtask.Core;

namespace PataNext.Export.Godot;

public partial class EntryModule : HostModule
{
    private HostRunnerScope hostScope;
    
    public EntryModule(HostRunnerScope scope) : base(scope)
    {
        hostScope = scope;
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

        LoadModule(sc => new PataNext.Module.Abilities.Module(sc));
        
        TrackDomain((SimulationDomain domain) =>
        {
            domain.JobRunner.CompleteBatch(domain.CurrentJob);
            
            new RhythmEnginePresentation(domain.Scope);
            new UnitPresentation(domain.Scope);
            new DefaultMarchScript(domain.Scope);


            // Create a random player entity
            //  . It contains the input for the rhythm engine
            var player = domain.GameWorld.CreateEntity();
            domain.GameWorld.AddComponent(player, PlayerDescription.Type.GetOrCreate(domain.GameWorld), default);
            domain.GameWorld.AddComponent(player, GameRhythmInput.Type.GetOrCreate(domain.GameWorld), default);

            // Create a random rhythm engine
            //  . Start in 2 seconds
            //  . Has 4 beats (0.5s interval) per commands
            //  . Modify combo settings
            //  . Set the player as a relative
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

            // Create the cursor entity
            // # The cursor could be considered as an invisible Hatapon
            // # It controls the default target of an unit (an ability can ignore it)
            var cursor = domain.GameWorld.CreateEntity();
            domain.GameWorld.AddComponent(cursor, CursorLayout.ToComponentType(domain.GameWorld), default);
            
            for (var i = 0; i < 1; i++)
            {
                // Create an unit
                //  . We give it relations to the player, engine and cursor entities
                //  . We make it able to receive and execute abilities (with OwnerActiveAbility)
                //  . The position is a little bit modified if we have a lot of units (to discern them)
                //  . We make it face and move in the right direction
                //  . The first unit will control the cursor
                //  . Then we add some abilities to it
                var unit = domain.GameWorld.CreateEntity();
                domain.GameWorld.AddComponent(unit, PlayableUnitLayout.ToComponentType(domain.GameWorld), default);
                domain.GameWorld.AddComponent(unit, PlayerDescription.Relative.ToComponentType(domain.GameWorld),
                    player);
                domain.GameWorld.AddComponent(unit, RhythmEngineDescription.Relative.ToComponentType(domain.GameWorld),
                    engine);
                domain.GameWorld.AddComponent(unit, CursorDescription.Relative.ToComponentType(domain.GameWorld),
                    cursor);
                domain.GameWorld.AddComponent(unit, OwnerActiveAbility.ToComponentType(domain.GameWorld), default);

                domain.GameWorld.GetPositionComponent(unit).X = i * 1.5f;
                domain.GameWorld.GetUnitPlayState(unit) = new UnitPlayState
                {
                    MovementSpeed = 1.2f,
                    MovementReturnSpeed = 1,
                    Weight = 10,
                };
                domain.GameWorld.AddUnitDirection(unit, new UnitDirection(1));

                // The first unit control the cursor
                if (i == 0)
                    domain.GameWorld.AddComponent(unit, CursorControlTag.ToComponentType(domain.GameWorld));

                domain.TaskScheduler.StartUnwrap(async () =>
                {
                    await Task.Delay(500);

                    domain.Scope.Context.TryGet(out AbilitySpawner spawner);

                    void SpawnAbility<T>()
                        where T : IRevolutionComponent
                    {
                        var type = T.ToComponentType(domain.GameWorld);
                        if (!spawner.TrySpawnAbility(type, domain.GameWorld.Safe(unit), out _))
                            throw new KeyNotFoundException(
                                $"Ability '{domain.GameWorld.ComponentTypeBoard.Names[type.Handle]}' not found"
                            );
                    }

                    SpawnAbility<DefaultMarchAbility>();
                    SpawnAbility<DefaultJumpAbility>();
                });
            }
        });

    }
}