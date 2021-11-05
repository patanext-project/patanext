using System;
using System.Threading.Tasks;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;
using ZLogger;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	public class UnitUpdateStatusEffectSystem : GameAppSystem, IPreUpdateSimulationPass
	{
		private readonly PooledList<ComponentReference> settingsRef, stateRefs;

		private ILogger           logger;
		private IManagedWorldTime worldTime;

		public UnitUpdateStatusEffectSystem([NotNull] WorldCollection collection) : base(collection)
		{
			AddDisposable(stateRefs   = new PooledList<ComponentReference>());
			AddDisposable(settingsRef = new PooledList<ComponentReference>());

			DependencyResolver.Add(() => ref logger);
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery query;

		public void OnBeforeSimulationUpdate()
		{
			query ??= CreateEntityQuery(new[]
			{
				typeof(UnitDescription)
			});

			var dt = (float) worldTime.Delta.TotalSeconds;

			foreach (var entity in query)
			{
				stateRefs.Clear();
				settingsRef.Clear();
				GameWorld.GetComponentOf(entity, AsComponentType<StatusEffectStateBase>(), stateRefs);
				GameWorld.GetComponentOf(entity, AsComponentType<StatusEffectSettingsBase>(), settingsRef);

				if (stateRefs.Count != settingsRef.Count)
				{
					logger.ZLogWarning("Entity {0} was set with invalid status effects (state_count={1} settings_count={2})", entity, stateRefs.Count, settingsRef.Count);
					continue;
				}

				var length = stateRefs.Count;
				//Console.WriteLine($"StatusEffect({entity}, {length})");
				for (var i = 0; i < length; i++)
				{
					ref var          state    = ref GameWorld.GetComponentData<StatusEffectStateBase>(entity, stateRefs[i].Type);
					ref readonly var settings = ref GameWorld.GetComponentData<StatusEffectSettingsBase>(entity, settingsRef[i].Type);
					if (state.Type != settings.Type)
					{
						logger.ZLogWarning("Entity {0} has an invalid status match (i={1} state_type={2} settings_type={3})", entity, i, state.Type, settings.Type);
						continue;
					}

					var resistanceGainFactor = 1f;
					var expLossFactor        = 0.01f;
					// The further we are to 0 from negative standpoint, the more the immunity should stay and the faster the resistance should recharge
					if (state.CurrentResistance < -settings.Resistance)
					{
						var f = MathUtils.RcpSafe(state.CurrentResistance / -settings.Resistance);
						expLossFactor        *= f;
						resistanceGainFactor *= Math.Max(1 + (1 - f * 2f), 1);
						
						// TODO: Perhaps there should be a better way with the distance? (bow units would have a disavantage)
						if (TryGetComponentData(entity, out UnitEnemySeekingState seekingState) && seekingState.Enemy == default)
							resistanceGainFactor *= 1.5f;
					}
					else
					{
						// TODO: Perhaps there should be a better way with the distance? (bow units would have a disavantage)
						if (TryGetComponentData(entity, out UnitEnemySeekingState seekingState) && seekingState.Enemy == default)
							resistanceGainFactor *= 1.5f;
					}
/*					
					if (settings.Resistance > state.CurrentResistance)
						Console.WriteLine($"    Type={state.Type} Resist={state.CurrentResistance:F2}% Power={state.CurrentPower}% Immunity={state.CurrentImmunity:F2}% ({expLossFactor:F3}; {resistanceGainFactor:F3})");
*/
					var previous = state.CurrentResistance;
					if (previous > settings.Resistance)
					{
						state.CurrentResistance = Math.Max(settings.Resistance, state.CurrentResistance - state.CurrentRegenPerSecond * dt);
					}
					else
					{
						state.CurrentResistance = Math.Min(settings.Resistance, state.CurrentResistance + state.CurrentRegenPerSecond * resistanceGainFactor * dt);
					}

					state.CurrentImmunity =  MathUtils.LerpNormalized(state.CurrentImmunity, 0, state.ImmunityExp);
					
					state.ImmunityExp     += dt * state.CurrentRegenPerSecond * expLossFactor;
				}
			}
		}
	}
}