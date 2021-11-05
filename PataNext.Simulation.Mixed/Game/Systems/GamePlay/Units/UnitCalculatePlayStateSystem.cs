using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	public class UnitCalculatePlayStateSystem : GameLoopAppSystem, IUpdateSimulationPass
	{
		private readonly PooledList<ComponentReference> settingsRef, stateRefs;
		
		public UnitCalculatePlayStateSystem(WorldCollection collection) : base(collection, false)
		{
			AddDisposable(stateRefs   = new PooledList<ComponentReference>());
			AddDisposable(settingsRef = new PooledList<ComponentReference>());
			
			Add((GameEntityHandle entity, ref UnitPlayState state, ref UnitStatistics original) =>
			{
				stateRefs.Clear();
				settingsRef.Clear();
				GameWorld.GetComponentOf(entity, AsComponentType<StatusEffectStateBase>(), stateRefs);
				GameWorld.GetComponentOf(entity, AsComponentType<StatusEffectSettingsBase>(), settingsRef);

				if (stateRefs.Count != settingsRef.Count)
				{
					return true;
				}

				var length = stateRefs.Count;
				for (var i = 0; i < length; i++)
				{
					ref var          effectState    = ref GameWorld.GetComponentData<StatusEffectStateBase>(entity, stateRefs[i].Type);
					ref readonly var effectSettings = ref GameWorld.GetComponentData<StatusEffectSettingsBase>(entity, settingsRef[i].Type);
					if (effectState.Type != effectSettings.Type)
					{
						continue;
					}

					effectState.CurrentRegenPerSecond   = effectSettings.RegenPerSecond;
					effectState.CurrentPower            = effectSettings.Power;
					effectState.ReceivedPowerPercentage = 1;
				}

				GameCombo.State    comboState    = default;
				GameCombo.Settings comboSettings = default;
				if (TryGetComponentData(entity, out Relative<RhythmEngineDescription> engineRelative)
				    && engineRelative.Target != default)
				{
					comboState    = GetComponentData<GameCombo.State>(engineRelative.Handle);
					comboSettings = GetComponentData<GameCombo.Settings>(engineRelative.Handle);
				}

				state.MovementSpeed       = comboSettings.CanEnterFever(comboState) ? original.FeverWalkSpeed : original.BaseWalkSpeed;
				state.Defense             = original.Defense;
				state.Attack              = original.Attack;
				state.AttackSpeed         = original.AttackSpeed;
				state.AttackSeekRange     = original.AttackSeekRange;
				state.MovementAttackSpeed = original.MovementAttackSpeed;
				state.MovementReturnSpeed = comboSettings.CanEnterFever(comboState) ? original.MovementAttackSpeed * 1.7f : original.MovementAttackSpeed;
				state.Weight              = original.Weight;
				state.KnockbackPower      = original.KnockbackPower;

				state.ReceiveDamagePercentage = 1;

				return true;
			});
		}

		public void OnSimulationUpdate() => RunExecutors();
	}
}