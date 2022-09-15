using System.Threading;
using System.Threading.Tasks;
using PataNext.Game.Client.Core.Inputs;
using PataNext.Game.Modules.Abilities.Components;
using PataNext.Module.Abilities.Providers.Defaults;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Common.GameMode;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.Units;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;
using revghost.Domains.Time;
using revghost.Injection.Dependencies;

namespace PataNext.Game.Modules.GameModes.Yarida64;

public class Yarida64GameModeSystem : GameModeSystemBase
{
    private IManagedWorldTime worldTime;
    
    public Yarida64GameModeSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref worldTime);
    }

    protected override void GetComponentTypes<TList>(TList all, TList none, TList or)
    {
        all.Add(Yarida64GameMode.ToComponentType(Simulation));
    }

    private UEntityHandle[] Yarida;
    private UEntityHandle UberHero;

    protected override async Task<bool> GetStateMachine(UEntitySafe gameModeEntity, CancellationToken token)
    {
        ref Yarida64GameMode data() => ref Simulation.GetYarida64GameMode(gameModeEntity.Handle);

        var count = Simulation.GetYarida64GameMode(gameModeEntity.Handle).YaridaCount;
        Yarida = new UEntityHandle[count];
        for (var i = 0; i < count; i++)
        {
            Yarida[i] = SpawnYarida(10, 10 + i * 10);
        }

        UberHero = SpawnUberHero(-5);
        
        while (!token.IsCancellationRequested)
        {
            // Waiting Phase
            while (!token.IsCancellationRequested)
            {
                if (Simulation.GetPositionComponent(UberHero).X >= 0)
                {
                    data() = data() with
                    {
                        Phase = Yarida64GameMode.EPhase.March,
                        YaridaOvertakeCount = -1
                    };

                    foreach (var unit in Yarida)
                    {
                        Simulation.GetUnitPlayState(unit).MovementReturnSpeed = 1;
                    }
                    
                    break;
                }

                foreach (var unit in Yarida)
                {
                    Simulation.GetUnitPlayState(unit).MovementSpeed = 0;
                    Simulation.GetPositionComponent(unit).X = 10;
                }

                await Task.Yield();
            }
            
            // Play Phase
            while (!token.IsCancellationRequested)
            {
                await Task.Yield();
            }
            
            await Task.Yield();
        }

        return true;
    }

    protected override void OnCrash(UEntitySafe gameModeEntity, Exception exception)
    {
        Console.WriteLine("noooooo the gamemode crashed :(");
    }

    private UEntityHandle SpawnUberHero(float positionX)
    {
        var player = Simulation.CreateEntity();
        Simulation.AddPlayerDescription(player);
        Simulation.AddGameRhythmInput(player);

        var cursor = Simulation.CreateEntity();
        Simulation.AddComponent(cursor, CursorLayout.ToComponentType(Simulation));
        Simulation.GetPositionComponent(cursor).X = positionX;
        Simulation.AddPlayerDescriptionRelative(cursor, player);

        var engine = Simulation.CreateEntity();
        Simulation.AddComponent(engine, RhythmEngineLayout.ToComponentType(Simulation));
        Simulation.GetRhythmEngineController(engine) = new RhythmEngineController
        {
            State = RhythmEngineController.EState.Playing,
            StartTime = worldTime.Total.Add(TimeSpan.FromSeconds(1))
        };
        Simulation.GetRhythmEngineSettings(engine) = new RhythmEngineSettings
        {
            BeatInterval = TimeSpan.FromSeconds(0.5f),
            MaxBeats = 4
        };
        Simulation.GetGameComboSettings(engine) = new GameComboSettings
        {
            MaxComboToReachFever = 9,
            RequiredScoreStart = 4,
            RequiredScoreStep = 0.5f
        };
        Simulation.AddPlayerDescriptionRelative(engine, player);

        var unit = Simulation.CreateEntity();
        Simulation.AddComponent(unit, PlayableUnitLayout.ToComponentType(Simulation));
        Simulation.AddPlayerDescriptionRelative(unit, player);
        Simulation.AddRhythmEngineDescriptionRelative(unit, engine);
        Simulation.AddCursorDescriptionRelative(unit, cursor);
        Simulation.AddOwnerActiveAbility(unit);

        Simulation.GetPositionComponent(unit).X = positionX;
        Simulation.GetUnitPlayState(unit) = new UnitPlayState
        {
            MovementSpeed = 1.2f,
            MovementAttackSpeed = 1f,
            MovementReturnSpeed = 1,
            Weight = 10,
        };
        Simulation.AddUnitDirection(unit, new UnitDirection(1));
        Simulation.AddCursorControlTag(unit);

        void SpawnAbility<T>()
            where T : IRevolutionComponent
        {
            var ability = Simulation.CreateEntity();
            Simulation.AddSetSpawnAbility(ability, new SetSpawnAbility(T.ToComponentType(Simulation), Simulation.Safe(unit)));
        }
        
        SpawnAbility<DefaultMarchAbility>();
        SpawnAbility<DefaultJumpAbility>();
        
        return unit;
    }

    private UEntityHandle SpawnYarida(float initialPosX, float positionX)
    {
        var unit = Simulation.CreateEntity();
        Simulation.AddComponent(unit, PlayableUnitLayout.ToComponentType(Simulation));
        // reset all statistics and only put weight
        Simulation.GetUnitPlayState(unit) = new UnitPlayState
        {
            Weight = 8.5f
        };
        Simulation.GetPositionComponent(unit).X = initialPosX;

        var cursor = Simulation.CreateEntity();
        Simulation.AddComponent(cursor, CursorLayout.ToComponentType(Simulation));
        Simulation.GetPositionComponent(cursor).X = positionX + initialPosX;

        Console.WriteLine($"spawn at {initialPosX}");
        
        Simulation.AddCursorDescriptionRelative(unit, cursor);
        
        return unit;
    }
}