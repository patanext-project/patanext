using DefaultEcs;
using PataNext.Game.Client.Core.Inputs;
using Quadrum.Game.Modules.Client.Audio;
using Quadrum.Game.Modules.Client.Audio.Client;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs;
using revecs.Core;
using revghost;
using revghost.Injection.Dependencies;
using revghost.IO.Storage;
using revghost.Utility;

namespace PataNext.Game.Client.RhythmEngineAudio.Systems;

public partial class ShoutDrumSystem : PresentationRhythmEngineSystemBase
{
    private AudioClient client;
    private IStorage storage;
    
    public ShoutDrumSystem(Scope scope) : base(scope)
    {
        Dependencies.AddRef(() => ref client);
        Dependencies.AddRef(() => ref storage);
    }

    // for now normal drum sounds (no perfect/fail)
    private Dictionary<int, Dictionary<int, Entity>> audioOnPressureDrum = new();
    private Entity audioOnPerfect = new();
    
    private AudioPlayerEntity audioPlayer, perfectAudioPlayer;

    private PlayerQuery playerQuery;

    protected override void OnInit()
    {
        base.OnInit();

        playerQuery = new PlayerQuery(GameWorld);

        audioPlayer = client.CreatePlayer();
        perfectAudioPlayer = client.CreatePlayer();

        for (var key = 1; key <= 4; key++)
        {
            audioOnPressureDrum[key] = new Dictionary<int, Entity>();

            for (var rank = 0; rank < 3; rank++)
            {
                var path = $"RhythmEngineAudio/Resources/Drums/drum_{key}_{rank}.ogg";
                
                var audioFile = GetFirstFile(path);
                if (audioFile == null)
                {
                    HostLogger.Output.Warn($"Audio `{path}` not found");
                    continue;
                }

                audioOnPressureDrum[key][rank] = client.CreateAudio(audioFile);
            }

            audioOnPerfect = client.CreateAudio(GetFirstFile("RhythmEngineAudio/Resources/on_perfect.wav"));
        }
        
        perfectAudioPlayer.SetAudio(audioOnPerfect);
    }

    protected override void OnUpdatePass(EngineQuery.Iteration engine, GameTime time)
    {
        if (!playerQuery.Any())
            return;

        ref readonly var input = ref playerQuery.First().rhythmInput;

        var score = 0;
        // TODO: 0.16f is a magic value for now, but it's the threshold for perfect pressures
        if (Math.Abs(RhythmUtility.GetScore(engine.State, engine.Settings)) >= 0.16f)
        {
            score = 1;
        }

        if (engine.Recovery.IsRecovery(RhythmUtility.GetFlowBeat(engine.State, engine.Settings)))
        {
            score = 2;
        }

        // this is for slider (maybe remove it if sliders aren't here anymore)
        var isFirstInput = engine.Progress.Count == 0
                           && (engine.Executing.CommandTarget.Equals(default)
                               || engine.Executing.ActivationBeatStart <=
                               RhythmUtility.GetActivationBeat(engine.State, engine.Settings));

        for (var i = 0; i < input.Actions.Length; i++)
        {
            ref readonly var action = ref input.Actions[i];
            // TODO: handle sliders later (if they're still needed for patanext)
            // HACK: (the plus two is a hack for now, it will be removed once proper inputs will be done)
            if (action.InterFrame.Pressed != time.Frame)
                continue;

            // TODO: later hero mode and sliders(?)
            var resource = audioOnPressureDrum[i + 1][score];
            
            audioPlayer.SetAudio(resource);
            audioPlayer.Play();
            
            if (engine.Executing.PowerInteger >= 100
                && engine.Executing.ActivationBeatStart >= RhythmUtility.GetActivationBeat(engine.State, engine.Settings))
            {
                perfectAudioPlayer.Play();
            }
        }
    }

    // TODO: move it to the hosting framework
    private IFile? GetFirstFile(string pattern)
    {
        using var files = storage.GetPooledFiles(pattern);
        return files.FirstOrDefault();
    }

    private partial struct PlayerQuery : IQuery<(
        Read<GameRhythmInput> rhythmInput,
        All<PlayerDescription>)>
    {
        
    }
}