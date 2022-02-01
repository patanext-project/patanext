using System.Text.Json;
using GameHost.Native.Char;
using revecs.Core;
using revghost.Shared.Events;

namespace PataNext.Game.Client.RhythmEngineAudio.BGM.Directors;

public class BgmDefaultDirector : BgmDirectorBase
{
    private readonly Dictionary<int, int> commandCycle;
    public readonly Bindable<int> HeroModeCombo;
    public readonly Bindable<IncomingCommandData> IncomingCommand;
    public readonly Bindable<bool> IsFever;

    public BgmDefaultDirector(JsonElement elem, BgmStore store, BgmDirectorBase parent) : base(elem, store, parent)
    {
        Loader = new BgmDefaultSamplesLoader(store);
        IncomingCommand = new Bindable<IncomingCommandData>();
        IsFever = new Bindable<bool>();
        HeroModeCombo = new Bindable<int>();

        commandCycle = new Dictionary<int, int>();
    }

    public int GetNextCycle(CharBuffer64 commandId, string state, int? wantedCycle)
    {
        if (!(Loader.GetCommand(commandId) is BgmDefaultSamplesLoader.ComboBasedCommand command))
            return 0;

        var hash = commandId.GetHashCode();
        if (!commandCycle.ContainsKey(hash))
            commandCycle.Add(hash, -1);

        var cycle = wantedCycle ?? commandCycle[hash] + 1;

        /*if (cycle >= command.mappedFile[state].Count)
            cycle = 0;*/
        cycle %= command.mappedFile[state].Count;

        commandCycle[hash] = cycle;
        return cycle;
    }

    public (string type, int index) GetNextTrack(bool isEntrance, bool isFever, int combo)
    {
        if (!(Loader.GetSoundtrack() is BgmDefaultSamplesLoader.SlicedSoundTrack soundTrack))
            return default;

        var type = isEntrance switch
        {
            true => isFever switch
            {
                true => "fever_entrance",
                false => "before_entrance"
            },
            false => isFever switch
            {
                true => "fever",
                false => "before"
            }
        };

        var files = type switch
        {
            "fever_entrance" => soundTrack.FeverEntrance,
            "before_entrance" => soundTrack.BeforeEntrance,
            "fever" => soundTrack.Fever,
            "before" => soundTrack.Before
        };

        return (type, combo % files.Length);
    }

    public struct IncomingCommandData
    {
        public UEntitySafe CommandId;
        public TimeSpan Start, End;
    }
}