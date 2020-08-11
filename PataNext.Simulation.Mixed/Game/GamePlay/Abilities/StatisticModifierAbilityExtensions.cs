using System;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;

namespace PataNext.Module.Simulation.Game.GamePlay.Abilities
{
	public static class StatisticModifierAbilityExtensions
	{
		public static void Multiply(this StatisticModifier stat, ref UnitPlayState playState)
		{
			void mul_float(ref float left, in float multiplier)
			{
				left *= multiplier;
			}

			void mul_int(ref int left, in float multiplier)
			{
				var originalF = left * multiplier;
				left += ((int) Math.Round(originalF) - left);
			}

			mul_int(ref playState.Attack, stat.Attack);
			mul_int(ref playState.Defense, stat.Defense);

			mul_float(ref playState.ReceiveDamagePercentage, stat.ReceiveDamage);

			mul_float(ref playState.MovementSpeed, stat.MovementSpeed);
			mul_float(ref playState.MovementAttackSpeed, stat.MovementAttackSpeed);
			mul_float(ref playState.MovementReturnSpeed, stat.MovementReturnSpeed);
			mul_float(ref playState.AttackSpeed, stat.AttackSpeed);

			mul_float(ref playState.AttackSeekRange, stat.AttackSeekRange);

			mul_float(ref playState.Weight, stat.Weight);
		}
	}
}