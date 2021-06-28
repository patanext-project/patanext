using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;

namespace PataNext.Module.Simulation.Game.GamePlay.Damage
{
	public struct DamageFrameData : IComponentData
	{
		public int Attack;

		/// <summary>
		/// The weight of the creator of this event. Can be used for pushing or knockBack
		/// </summary>
		public float Weight;

		public float KnockBackPower;

		/// <summary>
		/// How much [0-1] this event will ignore <see cref="UnitPlayState.Defense"/>
		/// </summary>
		public float IgnoreDefense;

		/// <summary>
		/// How much [0-1] this event will ignore <see cref="UnitPlayState.ReceiveDamagePercentage"/>
		/// </summary>
		public float IgnoreReceiveDamage;

		public DamageFrameData(UnitPlayState fromUnit)
		{
			Attack = fromUnit.Attack;
			Weight = fromUnit.Weight;

			KnockBackPower = fromUnit.KnockbackPower;
			IgnoreDefense  = 0;
			// todo: perhaps check for UnitPlayState.DamageBonus`Type ?
			IgnoreReceiveDamage = 0;
		}

		public bool CanKnockBack(in float weight, out float power)
		{
			power = KnockBackPower - weight;
			return power > 0;
		}
	}

	public struct DamageFrameDataStatusEffect : IComponentBuffer
	{
		public ComponentType Type;
		public float         Power;

		public DamageFrameDataStatusEffect(ComponentType type, float power)
		{
			Type  = type;
			Power = power;
		}
	}
}