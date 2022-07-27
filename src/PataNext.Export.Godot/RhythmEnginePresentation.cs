using System.Runtime.InteropServices;
using System.Xml;
using Collections.Pooled;
using GodotCLR;
using PataNext.Export.Godot.Presentation;
using PataNext.Game.Client.Core.Inputs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs;
using revecs.Core;
using revecs.Extensions.Buffers;
using revghost;
using revtask.Core;

namespace PataNext.Export.Godot;

// For now it's used for getting inputs
public partial class RhythmEnginePresentation : PresentationGodotBaseSystem
{
    private GD.PackedScene _packedScene;
    
    public RhythmEnginePresentation(Scope scope) : base(scope)
    {
        _packedScene = GD.ResourceLoader.Load("res://rhythm_engine.tscn")
            .To<GD.PackedScene>();
    }

    private EngineQuery engineQuery;
    private PlayerQuery playerQuery;
    private TimeQuery timeQuery;

    protected override void GetMatchedComponents(PooledList<ComponentType> all, PooledList<ComponentType> or,
        PooledList<ComponentType> none)
    {
        engineQuery = new EngineQuery(GameWorld);
        playerQuery = new PlayerQuery(GameWorld);
        timeQuery = new TimeQuery(GameWorld);

        all.Add(RhythmEngineSettings.Type.GetOrCreate(GameWorld));
        all.Add(RhythmEngineState.Type.GetOrCreate(GameWorld));
    }

    protected override bool EntityMatch(in UEntityHandle entity)
    {
        return true;
    }

    protected override bool OnSetPresentation(in UEntitySafe entity, out JobRequest jobRequest)
    {
        jobRequest = NewInstantiateJob(entity, _packedScene);
        return true;
    }

    protected override bool OnRemovePresentation(in UEntitySafe entity, in GD.Node node)
    {
        return true;
    }

    static string to_patapon_drum(int key)
    {
        return key switch
        {
            1 => "Pata",
            2 => "Pon",
            3 => "Don",
            4 => "Chaka",
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }

    protected override void OnPresentationLoop()
    {
        base.OnPresentationLoop();

        var player = playerQuery.First();
        if (player.Handle.Equals(default))
            throw new InvalidOperationException("null player");

        var time = timeQuery.First().GameTime;
        foreach (var entity in QueryWithPresentation)
        {
            if (!TryGetNode(entity, out var node))
                continue;

            while (node.Call("has_input_left", default).Bool)
            {
                var lastInput = (int) node.Call("get_last_input", default).Int;
                UtilityFunctions.Print($"Input: {(DefaultCommandKeys) lastInput}");
                
                // TODO: should be before the simulation update
                // HACK: (see todo) + 1 since the rhythm systems will receive the input on next frame
                player.Input.Actions[lastInput - 1].InterFrame.Pressed = time.Frame + 1;
            }

            var title = $"Rhythm Engine ({entity})";
            var currCommand = "Current:  ";
            var predicted = "Predicted:  \n";

            foreach (var engine in engineQuery)
            {
                title += "  Elapsed: " + (int) engine.state.Elapsed.TotalSeconds + "s";

                for (var i = 0; i < engine.progress.Count; i++)
                {
                    currCommand += to_patapon_drum(engine.progress[i].Value.KeyId);
                    if (i + 1 < engine.progress.Count)
                        currCommand += " ";
                }
                
                for (var i = 0; i < engine.predicted.Count; i++)
                {
                    var actionType = CommandActions.Type.GetOrCreate(GameWorld).UnsafeCast<RhythmCommandAction>();
                    var actions = GameWorld.ReadComponent(engine.predicted[i].Value.Handle, actionType);

                    for (var j = 0; j < actions.Length; j++)
                    {
                        predicted += to_patapon_drum(actions[j].Key);
                        if (j + 1 < actions.Length)
                            predicted += " ";
                    }

                    if (i + 1 < engine.predicted.Count)
                        predicted += "\n";
                }
            }
            
            node.SetProperty("title", new Variant(title));
            node.SetProperty("curr_command", new Variant(currCommand));
            node.SetProperty("predicted", new Variant(predicted));
        }
    }

    private partial struct EngineQuery : IQuery<(
        Read<RhythmEngineSettings> settings,
        Write<RhythmEngineState> state,
        Write<RhythmEngineRecoveryState> recovery,
        Write<GameCommandState> executing,
        Write<RhythmEngineCommandProgress> progress,
        Read<RhythmEnginePredictedCommands> predicted
        )>
    {
    }

    private partial struct PlayerQuery : IQuery<(Write<GameRhythmInput> Input, All<PlayerDescription>)>
    {
    }
    
    private partial struct TimeQuery : IQuery<Read<GameTime>>
    {
    }
}