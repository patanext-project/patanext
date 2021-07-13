using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Game.Abilities;

namespace PataNext.Module.Simulation.Game.GamePlay.Abilities
{
	public struct AbilityModifyStatsOnChaining : IComponentData
	{
		public StatisticModifier ActiveModifier;
		public StatisticModifier FeverModifier;
		public StatisticModifier PerfectModifier;

		public StatisticModifier ChargeModifier;
		public bool              SetChargeModifierAsFirst;
	}
}