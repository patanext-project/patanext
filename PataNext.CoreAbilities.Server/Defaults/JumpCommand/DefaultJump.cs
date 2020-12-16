using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed.Defaults;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.CoreAbilities.Server.Defaults.JumpCommand
{
	public class DefaultJump : AbilityScriptModule<DefaultJumpAbilityProvider>
	{
		private IManagedWorldTime worldTime;

		public DefaultJump(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

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

			const float startJumpTime = 0.5f;

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