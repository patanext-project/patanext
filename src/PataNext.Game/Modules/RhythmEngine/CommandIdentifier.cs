using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.RhythmEngine;

public partial struct CommandIdentifier : ISparseComponent
{
    public string Value;

    public CommandIdentifier(string value) => Value = value;
}