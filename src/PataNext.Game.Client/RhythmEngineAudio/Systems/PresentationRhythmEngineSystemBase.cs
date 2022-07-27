using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using revecs;
using revecs.Core;
using revghost;
using revghost.Domains.Time;
using revghost.Ecs;
using revghost.Injection.Dependencies;
using revghost.Loop.EventSubscriber;

namespace PataNext.Game.Client.RhythmEngineAudio.Systems;

public abstract partial class PresentationRhythmEngineSystemBase : AppSystem
{
    protected RevolutionWorld GameWorld;

    private EngineQuery engineQuery;
    private TimeQuery timeQuery;
    
    private IPresentationLoop updateLoop;
    
    protected PresentationRhythmEngineSystemBase(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref GameWorld!);
        Dependencies.Add(() => ref updateLoop);
    }

    protected override void OnInit()
    {
        engineQuery = new EngineQuery(GameWorld);
        timeQuery = new TimeQuery(GameWorld);
        
        Disposables.Add(updateLoop.Subscribe(OnUpdate));
    }

    private void OnUpdate()
    {
        if (!engineQuery.Any())
            return;
        if (!timeQuery.Any())
            return;

        // TODO: Once there will be multiplayer support (or any future need for multiple engines)
        //       update to only get the engine of the client player.
        OnUpdatePass(engineQuery.First(), timeQuery.First().GameTime);
    }

    protected abstract void OnUpdatePass(EngineQuery.Iteration engine, GameTime gameTime);

    protected partial struct EngineQuery : IQuery<(
        Read<RhythmEngineController> Controller,
        Read<RhythmEngineSettings> Settings,
        Read<RhythmEngineState> State,
        Read<RhythmEngineRecoveryState> Recovery,
        
        Read<RhythmEngineCommandProgress> Progress,
        Read<RhythmEngineExecutingCommand> Executing,
        
        Read<GameCommandState> CommandState,
        
        Read<GameComboState> ComboState,
        Read<GameComboSettings> ComboSettings)>
    {
        
    }

    // HACK: for now IQuery<T> only support tuples as arguments, in the future make it accept single elements.
    protected partial struct TimeQuery : IQuery<(Read<GameTime>, None<RhythmEngineController>)>
    {
        
    }
}