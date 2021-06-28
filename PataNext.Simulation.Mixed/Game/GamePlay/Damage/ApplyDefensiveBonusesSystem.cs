using System;
using System.Collections.Generic;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Game.Abilities;
using PataNext.Game.Abilities.Effects;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Damage
{
	[UpdateAfter(typeof(GenerateDamageRequestSystem))]
	[UpdateAfter(typeof(ApplyStatusSystem))]
	public class ApplyDefensiveBonusesSystem : GameAppSystem, IGameEventPass
	{
		private UnitStatusEffectComponentProvider statusProvider;

		public ApplyDefensiveBonusesSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref statusProvider);
		}

		private EntityQuery evQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			evQuery = CreateEntityQuery(new[] {typeof(TargetDamageEvent)});
		}


		public void OnGameEventPass()
		{
			var damageAccessor = GetAccessor<TargetDamageEvent>();
			foreach (var handle in evQuery)
			{
				ref var damageEvent = ref damageAccessor[handle];
				if (damageEvent.Damage >= 0)
					continue;

				if (!TryGetComponentData(damageEvent.Victim, out UnitPlayState playState))
					continue;

				var currentPiercingPower = 0f;
				if (TryGetComponentBuffer<DamageFrameDataStatusEffect>(handle, out var statusBuffer))
				{
					foreach (var status in statusBuffer)
						if (status.Type == AsComponentType<Piercing>() && status.Power > 0)
							currentPiercingPower = status.Power;
				}

				var piercedDefense        = 0f;
				var piercedReceivedDamage = 0f;
				if (statusProvider.HasStatus(damageEvent.Victim.Handle, AsComponentType<Piercing>()))
				{
					ref readonly var state    = ref statusProvider.GetStatusState(damageEvent.Victim.Handle, AsComponentType<Piercing>());
					ref readonly var settings = ref statusProvider.GetStatusSettings(damageEvent.Victim.Handle, AsComponentType<Piercing>());
					if (state.CurrentResistance < 0)
					{
						var leftLv2 = state.CurrentResistance + (settings.Resistance * 2);
						
						// --- Level one piercing (Defense is reduced based on how much resistance there is left, and current piercing power)
						piercedDefense += MathUtils.DivSafe(currentPiercingPower, leftLv2) * playState.Defense;

						var leftLv3 = state.CurrentResistance + (settings.Resistance * 6);
						if (leftLv2 < 0)
						{
							// --- Level two piercing (broken defense, and now ReceivedDamage start to be reduced)
							piercedDefense = playState.Defense;

							piercedReceivedDamage += MathUtils.DivSafe(currentPiercingPower, Math.Abs(leftLv3));
							if (leftLv3 < 0)
							{
								piercedReceivedDamage = 1;
							}
						}
					}
				}

				piercedDefense        = Math.Clamp(piercedDefense, 0, playState.Defense);
				
				playState.Defense                 = Math.Max(Math.Min(0, playState.Defense), playState.Defense - (int) piercedDefense); // floor
				playState.ReceiveDamagePercentage = Math.Max(playState.ReceiveDamagePercentage, Math.Min(1, playState.ReceiveDamagePercentage + piercedReceivedDamage));

				damageEvent.Damage = Math.Min(damageEvent.Damage + playState.Defense, 0);
				if (damageEvent.Damage != 0 && Math.Abs(playState.ReceiveDamagePercentage - 1) > 0.01f)
					damageEvent.Damage *= playState.ReceiveDamagePercentage;
			}
		}
	}
}