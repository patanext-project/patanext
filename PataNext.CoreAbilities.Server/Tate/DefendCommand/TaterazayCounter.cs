using System;
using System.Collections.Generic;
using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CTate;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Tate.DefendCommand
{
	public class TaterazayCounter : ScriptBase<TaterazayCounterAbilityProvider>
	{
		private struct ProcessedCounterHit : IComponentData {}
		
		private IManagedWorldTime worldTime;
		private IPhysicsSystem    physicsSystem;

		public TaterazayCounter(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref physicsSystem);
		}

		private EntityQuery dmgEventQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			dmgEventQuery = CreateEntityQuery(new[] {typeof(TargetDamageEvent)}, new [] {typeof(ProcessedCounterHit)});
		}

		protected override void OnSetup(GameEntity self)
		{
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			ref var abilitySettings        = ref GetComponentData<TaterazayCounterAbility>(self);
			ref var abilityState           = ref GetComponentData<TaterazayCounterAbility.State>(self);
			ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);
			
			ref var playState = ref GetComponentData<UnitPlayState>(owner);
			
			ref readonly var statistics = ref GetComponentData<UnitStatistics>(owner);
			ref readonly var position   = ref GetComponentData<Position>(owner).Value;
			
			GameWorld.RemoveComponent(self.Handle, AsComponentType<HitBox>());
			GetBuffer<HitBoxHistory>(self).Clear();

			if (!state.IsActiveOrChaining)
			{
				abilityState.DamageStock = 0;
				abilityState.StopAttack();
				return;
			}

			if (abilityState.PreviousActivation != state.ActivationVersion)
			{
				abilityState.PreviousActivation = state.ActivationVersion;
				abilityState.Cooldown           = default;
			}

			if (abilityState.IsAttackingAndUpdate(abilitySettings, worldTime.Total))
			{
				controlVelocity.StayAtCurrentPositionX(50);

				// Keep stacking damage
				foreach (var entity in dmgEventQuery)
				{
					var ev = GetComponentData<TargetDamageEvent>(entity);

					if (ev.Victim == owner && ev.Damage <= 0)
						abilityState.DamageStock += Math.Abs(ev.Damage * abilitySettings.SendBackDamageFactorAfterTrigger);
				}
				
				var meleeRange = Math.Max(statistics.AttackMeleeRange, 2);

				if (abilityState.CanAttackThisFrame(abilitySettings, worldTime.Total, new TimeSpan(1)))
				{
					var copy = playState;
					copy.Attack += (int) Math.Ceiling(abilityState.DamageStock);
					
					using var entitySettings = World.Mgr.CreateEntity();
					entitySettings.Set<Shape>(new PolygonShape(meleeRange + 0.5f, meleeRange * 0.5f));
			
					physicsSystem.AssignCollider(self.Handle, entitySettings);

					// attack code
					AddComponent(self, new HitBox(owner, default));
					GetComponentData<Position>(self).Value       = position + new Vector3(0, 0.1f + meleeRange * 0.5f, 0);
					GetComponentData<DamageFrameData>(self)      = new DamageFrameData(copy);
					GetComponentData<HitBoxAgainstEnemies>(self) = new HitBoxAgainstEnemies(GetComponentData<Relative<TeamDescription>>(owner).Target);

					abilityState.DamageStock = default;
				}
			}

			if (state.IsActive && abilityState.AttackStart == default)
			{
				// before the counter attack we halve incoming damage.
				playState.ReceiveDamagePercentage *= 0.5f;
				
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
				foreach (ref var entity in dmgEventQuery)
				{
					var ev = GetComponentData<TargetDamageEvent>(entity);
					if (ev.Victim == owner && ev.Damage <= 0)
					{
						abilityState.DamageStock += Math.Abs(ev.Damage * abilitySettings.SendBackDamageFactorOnTrigger);
						trigger             =  true;

						AddComponent(entity, new ProcessedCounterHit());
						
						entity = default;
					}
				}

				if (trigger)
					abilityState.TriggerAttack(worldTime.ToStruct());
			}
		}
	}
}