using System;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CMega;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Mega
{
	public class MegaponBasicSonicAttack : ScriptBase<MegaponBasicSonicAttackAbilityProvider>
	{
		private IManagedWorldTime          worldTime;
		private BouncingProjectileProvider projectileProvider;
		private ExecuteActiveAbilitySystem execute;

		public MegaponBasicSonicAttack(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref projectileProvider);
			DependencyResolver.Add(() => ref execute);
		}

		private float  dt;
		private Random random = new Random(Environment.TickCount);

		protected override void OnSetup(GameEntity self)
		{
			dt = (float) worldTime.Delta.TotalSeconds;
			random.Next();
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			ref var abilitySettings = ref GetComponentData<MegaponBasicSonicAttackAbility>(self);
			ref var abilityState    = ref GetComponentData<MegaponBasicSonicAttackAbility.State>(self);
			abilityState.Cooldown -= worldTime.Delta;

			ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);

			ref readonly var position  = ref GetComponentData<Position>(owner).Value;
			ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);
			ref readonly var direction = ref GetComponentData<UnitDirection>(owner);
			ref readonly var offset    = ref GetComponentData<UnitTargetOffset>(owner);

			if (!state.IsActiveOrChaining)
			{
				abilityState.StopAttack();
				return;
			}

			var throwOffset = new Vector2(direction.Value, 2.6f);
			if (state.IsActive)
			{
				var deltaPosition = PredictTrajectory.Simple(throwOffset - Vector2.UnitX * offset.Attack, abilitySettings.ThrowVelocity * direction.FactorX, abilitySettings.Gravity);
				var result        = RoutineGetNearOfEnemy(owner, deltaPosition, attackNearDistance: float.MaxValue, selfDistance: 4);
				if (result.Enemy != default)
				{
					if (result.CanTriggerAttack)
						abilityState.TriggerAttack(worldTime.ToStruct());

					if (abilityState.AttackStart == default)
						controlVelocity.SetAbsolutePositionX(result.Target.X, 20);

					controlVelocity.OffsetFactor = 0;
				}
			}

			// If true, we're currently in the attack phase
			if (abilityState.IsAttackingAndUpdate(abilitySettings, worldTime.Total))
			{
				// When we attack, we should stay at our current position, and add a very small deceleration 
				controlVelocity.StayAtCurrentPositionX(1);

				if (abilityState.CanAttackThisFrame(abilitySettings, worldTime.Total, TimeSpan.FromSeconds(playState.AttackSpeed)))
				{
					const int maxBounces = 3;

					// Todo: accuracy should be stored in UnitPlayState (so we could have spears/skills that would be more precise)
					var accuracy       = AbilityUtility.CompileStat(GetComponentData<AbilityEngineSet>(self), 0.15f, 1, 2.5f, 3);
					var throwOffsetXYZ = new Vector3(throwOffset, 0);
					execute.Post.Schedule(projectileProvider.SpawnAndForget, new BouncingProjectileCreate
					{
						Owner      = owner,
						Pos        = position + throwOffsetXYZ,
						Vel        = new Vector3 {X = abilitySettings.ThrowVelocity.X * direction.Value, Y = abilitySettings.ThrowVelocity.Y + accuracy * (float) random.NextDouble()},
						Gravity    = new Vector3(abilitySettings.Gravity, 0),
						MaxBounces = maxBounces,
						Duration   = TimeSpan.FromSeconds(4),
						Reflection = new Vector3(1f, 0.95f, 0),
						IsBox      = false,
						Graphic    = default
					}, default);
				}
			}
			else if (state.IsChaining)
				controlVelocity.StayAtCurrentPositionX(10);
		}

		public override void Dispose()
		{
			base.Dispose();

			projectileProvider = null;
		}
	}
}