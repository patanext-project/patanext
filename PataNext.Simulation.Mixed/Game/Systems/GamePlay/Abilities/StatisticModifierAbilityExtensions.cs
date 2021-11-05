using System;
using System.Collections.Generic;
using GameHost.Simulation.TabEcs;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Systems;

namespace PataNext.Module.Simulation.Game.GamePlay.Abilities
{
	public static class StatisticModifierAbilityExtensions
	{
		public static void Multiply<TList>(this in StatisticModifier stat, ref UnitPlayState playState, TList list = default, GameWorld gameWorld = null)
			where TList : IList<ComponentReference>
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
			mul_float(ref playState.KnockbackPower, stat.Knockback);

			if (gameWorld != null)
			{
				var count = list.Count;
				for (var i = 0; i < count; i++)
				{
					ref var status = ref gameWorld.GetComponentData<StatusEffectStateBase>(list[i]);
					foreach (var modifier in stat.StatusEffects.Span)
					{
						if (status.Type != modifier.Type)
							continue;

						mul_float(ref status.CurrentPower, modifier.Power);
						mul_float(ref status.ReceivedPowerPercentage, modifier.ReceivePower);
						mul_float(ref status.CurrentRegenPerSecond, modifier.RegenPerSecond);
					}
				}
			}
		}
	}
}