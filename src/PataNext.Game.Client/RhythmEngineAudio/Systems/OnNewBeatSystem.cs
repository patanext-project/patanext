using Collections.Pooled;
using DefaultEcs;
using Quadrum.Game.Modules.Client.Audio;
using Quadrum.Game.Modules.Client.Audio.Client;
using Quadrum.Game.Modules.Simulation.Application;
using revecs;
using revecs.Core;
using revecs.Querying;
using revghost;
using revghost.Ecs;
using revghost.Injection;
using revghost.Injection.Dependencies;
using revghost.IO.Storage;
using revghost.Utility;

namespace PataNext.Game.Client.RhythmEngineAudio.Systems;

public class OnNewBeatSystem : PresentationRhythmEngineSystemBase
{
    private IStorage _storage;
    private AudioClient _audio;
    
    private Entity newBeatAudio;
    private AudioPlayerEntity audioPlayer;
    
    public OnNewBeatSystem(Scope scope) : base(scope)
    {
        Dependencies.AddRef(() => ref _storage);
        Dependencies.AddRef(() => ref _audio);
    }
    
    protected override void OnInit()
    {
        base.OnInit();
        
        using var files = _storage.GetPooledFiles("RhythmEngineAudio/Resources/on_new_beat.ogg");
        if (files.Count == 0)
            throw new KeyNotFoundException($"`on_new_beat.ogg` not found!");
        
        newBeatAudio = _audio.CreateAudio(files[0]);
        audioPlayer = _audio.CreatePlayer();
        audioPlayer.SetAudio(newBeatAudio);
    }

    protected override void OnUpdatePass(EngineQuery.Iteration engine, GameTime time)
    {
        if (engine.State.CurrentBeat >= 0 && engine.State.NewBeatTick == time.Frame)
        {
            // Play audio on each positive beat
            // TODO: maybe we should do .PlayDelayed(500ms) to be more precise?
            //audioPlayer.Play();
            var delay = engine.Settings.BeatInterval;
            delay += time.Total;
            
            audioPlayer.PlayDelayed(delay);
        }
    }
}