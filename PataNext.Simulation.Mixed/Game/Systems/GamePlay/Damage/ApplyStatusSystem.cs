using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Collections.Pooled;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Damage
{
	[UpdateAfter(typeof(GenerateDamageRequestSystem))]
	public class ApplyStatusSystem : GameAppSystem, IGameEventPass
	{
		private UnitStatusEffectComponentProvider statusProvider;

		public ApplyStatusSystem([NotNull] WorldCollection collection) : base(collection)
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
				if (damageEvent.Damage >= 0 || !TryGetComponentBuffer<DamageFrameDataStatusEffect>(handle, out var statusBuffer))
					continue;
				
				foreach (var statusEffect in statusBuffer)
				{
					if (statusEffect.Power == 0 || !statusProvider.HasStatus(damageEvent.Victim.Handle, statusEffect.Type))
						continue;

					ref var          state    = ref statusProvider.GetStatusState(damageEvent.Victim.Handle, statusEffect.Type);
					ref readonly var settings = ref statusProvider.GetStatusSettings(damageEvent.Victim.Handle, statusEffect.Type);

					state.CurrentResistance -= Math.Max(0, statusEffect.Power - state.CurrentImmunity) * state.ReceivedPowerPercentage;
					// We let the possibility of having an overflow of immunity, but we shouldn't increase if it's the case.
					state.CurrentImmunity = Math.Min(Math.Max(state.CurrentImmunity, settings.Resistance), state.CurrentImmunity + statusEffect.Power * settings.ImmunityPerAttack);
					// Reset the Exp counter
					state.ImmunityExp = 0;
				}
			}
		}
	}
}