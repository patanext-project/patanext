using System;
using System.Collections.Generic;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CTate;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Tate.DefendCommand
{
	public class TaterazayCounter : ScriptBase<TaterazayCounterAbilityProvider>
	{
		private IManagedWorldTime worldTime;

		public TaterazayCounter(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery dmgEventQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			dmgEventQuery = CreateEntityQuery(new[] {typeof(TargetDamageEvent)});
		}

		protected override void OnSetup(GameEntity self)
		{
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			ref var ability         = ref GetComponentData<TaterazayCounterAbility>(self);
			ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);

			ref readonly var position = ref GetComponentData<Position>(owner).Value;
			
			GameWorld.RemoveComponent(self.Handle, AsComponentType<HitBox>());
			GetBuffer<HitBoxHistory>(self).Clear();

			if (!state.IsActiveOrChaining)
			{
				ability.DamageStock = 0;
				ability.StopAttack();
				return;
			}

			if (ability.PreviousActivation != state.ActivationVersion)
			{
				ability.PreviousActivation = state.ActivationVersion;
				ability.Cooldown           = default;
			}

			if (ability.IsAttackingAndUpdate(worldTime.Total))
			{
				controlVelocity.StayAtCurrentPositionX(50);

				// Keep stacking damage
				foreach (var entity in dmgEventQuery)
				{
					var ev = GetComponentData<TargetDamageEvent>(entity);

					if (ev.Victim == owner && ev.Damage <= 0)
						ability.DamageStock += Math.Abs(ev.Damage * ability.SendBackDamageFactorAfterTrigger);
				}
				
				if (ability.CanAttackThisFrame(worldTime.Total, new TimeSpan(1)))
				{
					var playState = GetComponentData<UnitPlayState>(owner);
					playState.Attack += (int) Math.Ceiling(ability.DamageStock);

					// attack code
					AddComponent(self, new HitBox(owner, default));
					GetComponentData<Position>(self).Value       = position + new Vector3(0, 1, 0);
					GetComponentData<UnitPlayState>(self)        = playState;
					GetComponentData<HitBoxAgainstEnemies>(self) = new HitBoxAgainstEnemies(GetComponentData<Relative<TeamDescription>>(owner).Target);

					ability.DamageStock = default;
				}
			}

			if (state.IsActive && ability.AttackStart == default)
			{
				var (enemy, _) = GetNearestEnemy(owner.Handle, 4, 8);
				if (enemy != default)
				{
					var targetPosition = GetComponentData<Position>(enemy).Value;
					controlVelocity.SetAbsolutePositionX(targetPosition.X, 10);
					controlVelocity.OffsetFactor = 0;
				}
				else
				{
					controlVelocity.StayAtCurrentPositionX(5);
				}

				var trigger = false;
				foreach (var entity in dmgEventQuery)
				{
					var ev = GetComponentData<TargetDamageEvent>(entity);
					Console.WriteLine(ev.Victim);
					if (ev.Victim == owner && ev.Damage <= 0)
					{
						ability.DamageStock += Math.Abs(ev.Damage * ability.SendBackDamageFactorOnTrigger);
						trigger             =  true;
					}
				}

				if (trigger)
					ability.TriggerAttack(worldTime.ToStruct());
			}
		}
	}
}