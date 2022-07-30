using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.RhythmEngine.Commands;

public partial record struct MarchCommand : ITagComponent;
public partial record struct AttackCommand : ITagComponent;
public partial record struct DefendCommand : ITagComponent;
public partial record struct JumpCommand : ITagComponent;
public partial record struct PartyCommand : ITagComponent;
public partial record struct SummonCommand : ITagComponent;