using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed.Defaults;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.CoreAbilities.Server.Defaults.JumpCommand
{
	public class DefaultJump : AbilityScriptModule<DefaultJumpAbilityProvider>, IAbilitySimulationPass
	{
		const float startJumpTime = 0.5f;
		
		private IManagedWorldTime worldTime;

		public DefaultJump(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		#region Non Authoritative
		private EntityQuery nonAuthoritativeQuery;

		// TODO: For non authoritative workflow (which will be rare), we perhaps need to find a better solution to do this?
		//			Like another system called PredictiveAbilitySystem<T>.OnPredict(Owner, Self, AbilityState)
		public void OnAbilitySimulationPass()
		{
			var ownerAccessor     = GetAccessor<Owner>();
			var abilityAccessor   = GetAccessor<DefaultJumpAbility>();
			var stateAccessor     = GetAccessor<AbilityState>();
			var engineSetAccessor = GetAccessor<AbilityEngineSet>();
			foreach (var handle in nonAuthoritativeQuery ??= CreateEntityQuery(new[] {typeof(DefaultJumpAbility), typeof(Owner)}))
			{
				if (HasComponent<SimulationAuthority>(ownerAccessor[handle].Target.Handle))
					continue;

				ref var          ability   = ref abilityAccessor[handle];
				ref readonly var engineSet = ref engineSetAccessor[handle];

				var delta = engineSet.Process.Elapsed - engineSet.CommandState.StartTimeSpan;
				if ((stateAccessor[handle].Phase & EAbilityPhase.ActiveOrChaining) != 0 && delta > TimeSpan.Zero)
				{
					ability.ActiveTime = (float) delta.TotalSeconds;
					ability.IsJumping  = ability.ActiveTime <= startJumpTime;
				}
				else
				{
					ability.ActiveTime = 0.0f;
					ability.IsJumping  = false;
				}
			}
		}

		#endregion

		private float dt;

		protected override void OnSetup(GameEntity self)
		{
			dt = (float) worldTime.Delta.TotalSeconds;
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			ref var          ability   = ref GetComponentData<DefaultJumpAbility>(self);
			ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);

			if (state.ActivationVersion != ability.LastActiveId)
			{
				ability.IsJumping    = false;
				ability.ActiveTime   = 0;
				ability.LastActiveId = state.ActivationVersion;
			}

			ref var velocity = ref GetComponentData<Velocity>(owner).Value;
			if (!state.IsActiveOrChaining)
			{
				if (ability.IsJumping)
				{
					velocity.Y = Math.Max(0, velocity.Y - 60 * (ability.ActiveTime * 2));
				}

				ability.ActiveTime = 0;
				ability.IsJumping  = false;
				return;
			}

			var wasJumping = ability.IsJumping;
			ability.IsJumping = ability.ActiveTime <= startJumpTime;

			if (!wasJumping && ability.IsJumping)
				velocity.Y = Math.Max(velocity.Y + 25, 30);
			else if (ability.IsJumping && velocity.Y > 0)
				velocity.Y = Math.Max(velocity.Y - 60 * dt, 0);

			if (ability.ActiveTime < 3.25f)
				velocity.X = MathUtils.LerpNormalized(velocity.X, 0, dt * (ability.ActiveTime + 1) * Math.Max(0, 1 + playState.Weight * 0.1f));

			if (!ability.IsJumping && velocity.Y > 0)
			{
				velocity.Y = Math.Max(velocity.Y - 10 * dt, 0);
				velocity.Y = MathUtils.LerpNormalized(velocity.Y, 0, 5 * dt);
			}

			ability.ActiveTime += dt;

			ref var unitController = ref GetComponentData<UnitControllerState>(owner);
			unitController.ControlOverVelocityX = ability.ActiveTime < 3.25f;
			unitController.ControlOverVelocityY = ability.ActiveTime < 2.5f;
		}
	}
}