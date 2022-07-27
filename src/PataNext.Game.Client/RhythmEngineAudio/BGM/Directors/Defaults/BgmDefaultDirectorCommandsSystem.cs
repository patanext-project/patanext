using DefaultEcs;
using PataNext.Game.Modules.RhythmEngine;
using Quadrum.Game.Modules.Client.Audio;
using Quadrum.Game.Modules.Client.Audio.Client;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using revecs;
using revghost;
using revghost.Injection.Dependencies;
using revghost.IO.Storage;

namespace PataNext.Game.Client.RhythmEngineAudio.BGM.Directors.Defaults;

public partial class BgmDefaultDirectorCommandsSystem : BgmDirectorSystemBase<BgmDefaultDirector, BgmDefaultSamplesLoader>
{
    private AudioClient audioClient;
    private IStorage storage;

    public BgmDefaultDirectorCommandsSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref audioClient);
        Dependencies.Add(() => ref storage);
    }

    private AudioClips clips;
    private AudioPlayerEntity audioPlayer;

    private CommandQuery commandQuery;
    private Commands cmd;

    protected override void OnInit()
    {
        base.OnInit();

        commandQuery = new CommandQuery(GameWorld);
        cmd = new Commands(GameWorld);

        clips.EnterFever = audioClient.CreateAudio(
            GetFirstFile("RhythmEngineAudio/Resources/on_enter_fever.wav")
        );
        clips.LostFever = audioClient.CreateAudio(
            GetFirstFile("RhythmEngineAudio/Resources/on_fever_lost.wav")
        );

        audioPlayer = audioClient.CreatePlayer();
    }

    private Dictionary<string, ComboBasedOutput> commandComboMap = new();

    protected override void OnUpdatePass(EngineQuery.Iteration engine, GameTime gameTime, BgmDefaultDirector director, BgmDefaultSamplesLoader loader)
    {
        LoadFiles(director, loader);

        var isHeroMode = false; // todo: hero mode

        var incomingCommand = director.IncomingCommand;
        if (!incomingCommand.Value.CommandId.Equals(engine.Executing.CommandTarget)
            || incomingCommand.Value.Start != engine.CommandState.StartTime)
        {
            incomingCommand.Value = new BgmDefaultDirector.IncomingCommandData
            {
                CommandId = engine.Executing.CommandTarget,
                Start = engine.CommandState.StartTime,
                End = engine.CommandState.EndTime
            };

            if (!engine.Executing.CommandTarget.Equals(default)
                && engine.CommandState.StartTime > TimeSpan.Zero)
            {
                var identifier = cmd.ReadCommandIdentifier(engine.Executing.CommandTarget.Handle).Value;
                // ewww nesting
                if (commandComboMap.TryGetValue(identifier, out var output))
                {
                    var key = "normal";
                    if (engine.ComboSettings.CanEnterFever(engine.ComboState))
                        key = "fever";
                    else if (engine.ComboState.Score > 1)
                        key = "prefever";

                    var doFeverShout = false;
                    if (key.Equals("fever"))
                    {
                        doFeverShout = !director.IsFever.Value;
                        director.IsFever.Value = true;
                    }
                    else
                    {
                        director.IsFever.Value = false;
                    }

                    Entity audioHandle = default;
                    if (doFeverShout)
                    {
                        audioHandle = clips.EnterFever;
                        director.HeroModeCombo.Value = 0;
                    }
                    else if (isHeroMode) // todo: hero mode
                    {
                        
                    }
                    else if (output.Map.TryGetValue(key, out var resourceMap)
                             && resourceMap.TryGetValue(
                                 director.GetNextCycle
                                 (
                                     identifier,
                                     key,
                                     engine.ComboState.Count
                                 ),
                                 out audioHandle 
                             ))
                    {
                        director.HeroModeCombo.Value = 0;
                    }

                    if (audioHandle != default)
                    {
                        audioPlayer.SetAudio(audioHandle);
                        audioPlayer.PlayDelayed(gameTime.Total +
                                                (engine.CommandState.StartTime - engine.State.Elapsed));
                    }
                }
            } else audioPlayer.Stop(); // ewwwww
        }
        else
        {
            if (director.IsFever.Value && !engine.ComboSettings.CanEnterFever(engine.ComboState))
            {
                director.IsFever.Value = false;

                var audioHandle = clips.LostFever;
                audioPlayer.SetAudio(audioHandle);
                audioPlayer.Play();
            }
        } 
    }

    private void LoadFiles(BgmDefaultDirector director, BgmDefaultSamplesLoader loader)
    {
        foreach (var iter in commandQuery)
        {
            var identifier = iter.CommandIdentifier.Value;
            if (string.IsNullOrEmpty(identifier))
                continue;

            if (!commandComboMap.TryGetValue(identifier, out var output))
                commandComboMap[identifier] = output = new ComboBasedOutput();

            if (output.Source is not {  } source)
            {
                output.Source = (BgmDefaultSamplesLoader.ComboBasedCommand) loader.GetCommand(identifier);
            }
            else
            {
                foreach (var (type, files) in source.mappedFile)
                {
                    if (!output.Map.TryGetValue(type, out var map))
                        output.Map[type] = map = new ComboBasedOutput.AudioHandleMap();

                    for (var i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        if (!map.ContainsKey(i))
                            map[i] = audioClient.CreateAudio(file);
                    }
                }
            }
        }
        
        // TODO: add a way to replace OnEnterFever and OnFeverLost sounds
    }

    // TODO: move it to the hosting framework
    private IFile? GetFirstFile(string pattern)
    {
        using var files = storage.GetPooledFiles(pattern);
        return files.FirstOrDefault();
    }

    private class ComboBasedOutput
    {
        public BgmDefaultSamplesLoader.ComboBasedCommand Source;
        public Dictionary<string, AudioHandleMap> Map = new();

        public class AudioHandleMap : Dictionary<int, Entity>
        {
        }
    }

    private struct AudioClips
    {
        public Entity EnterFever;
        public Entity LostFever;
    }

    private partial struct CommandQuery : IQuery<Read<CommandIdentifier>>
    {
    }

    private partial struct Commands : CommandIdentifier.Cmd.IRead
    {
        
    }
}