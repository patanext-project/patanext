using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Abilities.Components;

public partial struct AbilityModifyStatsOnChaining : ISparseComponent
{
    public StatisticModifier ActiveModifier;
    public StatisticModifier FeverModifier;
    public StatisticModifier PerfectModifier;

    public StatisticModifier ChargeModifier;
    public bool              SetChargeModifierAsFirst;
}