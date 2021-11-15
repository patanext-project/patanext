using Collections.Pooled;
using GodotCLR;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.RhythmEngine;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs;
using revecs.Core;
using revecs.Extensions.Buffers;
using revghost;
using RustTest;

namespace PataNext.Export.Godot;

// For now it's used for getting inputs
public partial class RhythmEnginePresentation : PresentationGodotBaseSystem
{
    public RhythmEnginePresentation(Scope scope) : base(scope)
    {
    }

    private EngineQuery _engineQuery;

    protected override void GetMatchedComponents(PooledList<ComponentType> all, PooledList<ComponentType> or,
        PooledList<ComponentType> none)
    {
        _engineQuery = new EngineQuery(GameWorld);

        all.Add(RhythmEngineSettings.Type.GetOrCreate(GameWorld));
        all.Add(RhythmEngineState.Type.GetOrCreate(GameWorld));
    }

    protected override bool EntityMatch(in UEntityHandle entity)
    {
        return true;
    }

    protected override bool OnSetPresentation(in UEntitySafe entity, out NodeProxy node)
    {
        node = new NodeProxy("Rhythm Engine", "res://rhythm_engine.tscn");
        return true;
    }

    protected override bool OnRemovePresentation(in UEntitySafe entity, in NodeProxy node)
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

        foreach (var entity in QueryWithPresentation)
        {
            var node = GameWorld.GetComponentData(entity, GenericType);
            while (node.Call("has_input_left").AsBool())
            {
                var lastInput = (int) node.Call("get_last_input").AsInt();
                GodotCLR.Godot.Print($"Input: {(DefaultCommandKeys) lastInput}");

                // literally a copy of OnInputForRhythmEngine
                // I didn't made it a system yet since inputs isn't really done (it's more of a rough sketch for now)
                foreach (var (settings, state, recovery, executing, buffer, predictedBuffer) in _engineQuery)
                {
                    var renderBeat = RhythmEngineUtility.GetFlowBeat(state.__ref, settings.__ref);
                    
                    var cmdChainEndFlow =
                        RhythmEngineUtility.GetFlowBeat(TimeSpan.FromMilliseconds(executing.ChainEndTimeMs),
                            settings.BeatInterval);
                    var cmdEndFlow = RhythmEngineUtility.GetFlowBeat(TimeSpan.FromMilliseconds(executing.EndTimeMs),
                        settings.BeatInterval);

                    // check for one beat space between inputs (should we just check for predicted commands? 'maybe' we would have a command with one beat space)
                    var failFlag1 = buffer.Count > 0
                                    && predictedBuffer.Count == 0
                                    && renderBeat > buffer[^1].Value.FlowBeat + 1
                                    && cmdChainEndFlow > 0;
                    // check if this is the first input and was started after the command input time
                    var failFlag3 = renderBeat > cmdEndFlow
                                    && buffer.Count == 0
                                    && cmdEndFlow > 0;
                    // check for inputs that were done after the current command chain
                    var failFlag2 = renderBeat >= cmdChainEndFlow
                                    && cmdChainEndFlow > 0;
                    failFlag2 = false; // this flag is deactivated for delayed reborn ability
                    var failFlag0 = cmdEndFlow > renderBeat && cmdEndFlow > 0;

                    if (failFlag0 || failFlag1 || failFlag2 || failFlag3)
                    {
                        GodotCLR.Godot.Print($"Failed {failFlag0} {failFlag1} {failFlag2} {failFlag3}");
                        
                        recovery.RecoveryActivationBeat = renderBeat + 1;
                        executing.__ref = default;
                        continue;
                    }

                    var pressure = new FlowPressure(lastInput, state.Elapsed, settings.BeatInterval)
                    {
                        IsSliderEnd = false
                    };
                    
                    buffer.Add(new RhythmEngineCommandProgress {Value = pressure});
                    state.LastPressure = pressure;

                    Console.WriteLine($"{state.Elapsed} {settings.BeatInterval} {pressure.FlowBeat}");
                }
            }

            var title = $"Rhythm Engine ({entity})";
            var currCommand = "Current:  ";
            var predicted = "Predicted:  \n";
            
            foreach (var (settings, state, recovery, executing, buffer, predictedBuffer) in _engineQuery)
            {
                title += "  Elapsed: " + (int) state.Elapsed.TotalSeconds + "s";
                
                for (var i = 0; i < buffer.Count; i++)
                {
                    currCommand += to_patapon_drum(buffer[i].Value.KeyId);
                    if (i + 1 < buffer.Count)
                        currCommand += " ";
                }

                Console.WriteLine(predictedBuffer.Count);
                for (var i = 0; i < predictedBuffer.Count; i++)
                {
                    var actionType = CommandActions.Type.GetOrCreate(GameWorld).UnsafeCast<RhythmCommandAction>();
                    var actions = GameWorld.ReadComponent(predictedBuffer[i].Value.Handle, actionType);
                    
                    for (var j = 0; j < actions.Length; j++)
                    {
                        predicted += to_patapon_drum(actions[j].Key);
                        if (j + 1 < actions.Length)
                            predicted += " ";
                    }

                    if (i + 1 < predictedBuffer.Count)
                        predicted += "\n";
                }
            }

            Variant.New(title, out var titleVariant);
            Variant.New(currCommand, out var currCommandVariant);
            Variant.New(predicted, out var predictedVariant);
            node.SetProperty("title", ref titleVariant);
            node.SetProperty("curr_command", ref currCommandVariant);
            node.SetProperty("predicted", ref predictedVariant);
        }
    }

    private partial struct EngineQuery : IQuery,
        Read<RhythmEngineSettings>,
        Write<RhythmEngineState>,
        Write<RhythmEngineRecoveryState>,
        Read<GameCommandState>,
        Write<RhythmEngineCommandProgress>,
        Read<RhythmEnginePredictedCommands>
    {
    }
}