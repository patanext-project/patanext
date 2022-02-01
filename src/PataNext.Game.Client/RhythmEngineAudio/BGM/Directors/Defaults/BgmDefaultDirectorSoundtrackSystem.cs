using Collections.Pooled;
using DefaultEcs;
using Quadrum.Game.Modules.Client.Audio;
using Quadrum.Game.Modules.Client.Audio.Client;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Utility;

namespace PataNext.Game.Client.RhythmEngineAudio.BGM.Directors;

public partial class
    BgmDefaultDirectorSoundtrackSystem : BgmDirectorSystemBase<BgmDefaultDirector, BgmDefaultSamplesLoader>
{
    // For now assume that all song are 4 beats
    // (this value wouldn't change in PataNext?)
    public const int SongBeatSize = 4;

    private AudioClient audioClient;

    public BgmDefaultDirectorSoundtrackSystem(Scope scope) : base(scope)
    {
        Dependencies.AddRef(() => ref audioClient);
    }

    private AudioPlayerEntity audioPlayer;
    private Commands cmd;

    protected override void OnInit()
    {
        base.OnInit();

        audioPlayer = audioClient.CreatePlayer();
        Disposables.Add(audioPlayer);

        cmd = new Commands(GameWorld);
    }

    // Key is the identifier (before_entrance, fever, ...)
    // Value is the audio resource entity
    private Dictionary<string, PooledList<Entity>> mappedAudioResources = new();

    private Entity lastClip;
    private BgmFeverState feverState;
    private int nextLoopTrigger; // used for looping on the same clip (mostly for the "before" key)

    protected override void OnUpdatePass(EngineQuery.Iteration engine, GameTime gameTime,
        BgmDefaultDirector director,
        BgmDefaultSamplesLoader loader)
    {
        LoadBgmFiles(loader);
        if (mappedAudioResources.Count == 0)
            return;

        var beforeEntranceCount = 0;
        if (mappedAudioResources
            .ContainsKey(
                "before_entrance")) // no try-get here (we don't want to leak the resulting var onto the outer scope)
            beforeEntranceCount = mappedAudioResources["before_entrance"].Count;

        if (engine.State.CurrentBeat < 0 || !cmd.HasRhythmEngineIsPlaying(engine.Handle))
        {
            audioPlayer.Stop();
            return;
        }

        var activationBeat = RhythmEngineUtility.GetActivationBeat(engine.State, engine.Settings);
        var flowBeat = RhythmEngineUtility.GetFlowBeat(engine.State, engine.Settings);

        // After initial entrance
        (string type, int index) track = default;
        if (activationBeat >= beforeEntranceCount * SongBeatSize * 2 || engine.ComboState.Count > 0)
        {
            // not in fever
            if (!engine.ComboSettings.CanEnterFever(engine.ComboState))
            {
                feverState = default;

                // For now we get the score in a rough way
                // This will change once the algorithm for combo score change
                var score = Math.Max(
                    (int) engine.ComboState.Score +
                    Math.Max(engine.ComboState.Score >= 1.9f ? 1 + engine.ComboState.Count : 0, 0),
                    engine.ComboState.Count);

                track = director.GetNextTrack(false, false, score);
            }
            // in fever
            else
            {
                track = director.GetNextTrack(true, true, 0);
                if (!feverState.IsActive)
                {
                    feverState.IsActive = true;
                    feverState.EndEntranceAtBeat = activationBeat
                                                   + mappedAudioResources["fever_entrance"].Count * SongBeatSize * 2;
                    feverState.ComboStart = engine.ComboState.Count;
                }
                else if (feverState.EndEntranceAtBeat < activationBeat)
                {
                    track = director.GetNextTrack(
                        false,
                        true,
                        Math.Max(0, engine.ComboState.Count - feverState.ComboStart - 1)
                    );
                }
            }
        }
        else
        {
            var currentCommandIdx = activationBeat != 0 ? activationBeat / SongBeatSize : 0;
            track = director.GetNextTrack(true, false, currentCommandIdx);
        }

        if (string.IsNullOrEmpty(track.type))
        {
            HostLogger.Output.Error("No track found?");
            return;
        }

        var targetAudio = mappedAudioResources[track.type][track.index];


        var cmdStartActivationBeat = RhythmEngineUtility.GetActivationBeat(
            engine.CommandState.StartTime,
            engine.Settings.BeatInterval
        );
        if (cmdStartActivationBeat >= activationBeat)
            activationBeat = cmdStartActivationBeat - 1;
        
        var nextBeatDelay = (activationBeat + 1) * engine.Settings.BeatInterval - engine.State.Elapsed;

        var hasSwitched = false;
        if (lastClip != targetAudio)
        {
            hasSwitched = Switch(targetAudio, nextBeatDelay);
            if (track.type == "before" && track.index == 0)
                nextLoopTrigger = activationBeat + SongBeatSize * 2;
            else
                nextLoopTrigger = -1;
        }

        // If we need to loop again on the same clip, do it
        if (audioPlayer.Original.Has<AudioResource>()
            && !hasSwitched && nextLoopTrigger > 0 && activationBeat >= nextLoopTrigger)
        {
            hasSwitched = Switch(targetAudio, nextBeatDelay);

            nextLoopTrigger = activationBeat + SongBeatSize * 2;
        }

        // TODO DEBUG: remove it later
        if (hasSwitched)
        {
            HostLogger.Output.Info($"Play {targetAudio} (key={track.type} idx={track.index})");
        }
    }

    private bool Switch(Entity targetAudio, TimeSpan delay)
    {
        var hasSwitched = false;
        lastClip = targetAudio;
        if (!targetAudio.Has<AudioResource>())
        {
            HostLogger.Output.Error($"AudioResource for {targetAudio} not loaded");
            audioPlayer.Stop();

            lastClip = default;
        }
        else
        {
            audioPlayer.SetAudio(targetAudio);
            audioPlayer.PlayDelayed(delay);
            
            HostLogger.Output.Info($"Play {targetAudio} in {delay.TotalSeconds:F3}s");

            hasSwitched = true;
        }

        return hasSwitched;
    }

    private void LoadBgmFiles(BgmSamplesLoaderBase loader)
    {
        // for now only support soundtrack that are sliced files
        if (loader.GetSoundtrack() is not BgmDefaultSamplesLoader.SlicedSoundTrack slicedSoundTrack)
            return;

        foreach (var (identifier, files) in slicedSoundTrack.mappedFile)
        {
            if (mappedAudioResources.ContainsKey(identifier))
                continue;

            var list = new PooledList<Entity>(files.Count);
            foreach (var file in files)
                list.Add(audioClient.CreateAudio(file));

            mappedAudioResources[identifier] = list;
        }
    }

    private partial struct Commands : RhythmEngineIsPlaying.Cmd.IRead
    {
    }

    private struct BgmFeverState
    {
        public bool IsActive;
        public int ComboStart;
        public int EndEntranceAtBeat;
    }
}