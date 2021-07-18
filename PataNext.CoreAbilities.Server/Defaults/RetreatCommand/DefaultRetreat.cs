using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed.Defaults;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Defaults.RetreatCommand
{
	public class DefaultRetreat : AbilityScriptModule<DefaultRetreatAbilityProvider>, IAbilitySimulationPass
	{
		const float walkbackTime = 3.0f;

		private IManagedWorldTime worldTime;

		public DefaultRetreat(WorldCollection collection) : base(collection)
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
			var abilityAccessor   = GetAccessor<DefaultRetreatAbility>();
			var stateAccessor     = GetAccessor<AbilityState>();
			var engineSetAccessor = GetAccessor<AbilityEngineSet>();
			foreach (var handle in nonAuthoritativeQuery ??= CreateEntityQuery(new[] {typeof(DefaultRetreatAbility), typeof(Owner)}))
			{
				if (HasComponent<SimulationAuthority>(ownerAccessor[handle].Target.Handle))
					continue;

				ref var          ability   = ref abilityAccessor[handle];
				ref readonly var engineSet = ref engineSetAccessor[handle];

				var delta = engineSet.Process.Elapsed - engineSet.CommandState.StartTimeSpan;
				if ((stateAccessor[handle].Phase & EAbilityPhase.ActiveOrChaining) != 0 && delta > TimeSpan.Zero)
				{
					ability.ActiveTime = (float) delta.TotalSeconds;
					ability.IsRetreating  = ability.ActiveTime <= walkbackTime;
				}
				else
				{
					ability.ActiveTime   = 0.0f;
					ability.IsRetreating = false;
				}
			}
		}

		#endregion

		private float dt;

		protected override void OnSetup(Span<GameEntityHandle> abilities)
		{
			dt = (float) worldTime.Delta.TotalSeconds;
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			ref var ability = ref GetComponentData<DefaultRetreatAbility>(self);

			if (state.ActivationVersion != ability.LastActiveId)
			{
				ability.IsRetreating = false;
				ability.ActiveTime   = 0;
				ability.LastActiveId = state.ActivationVersion;
			}

			ref readonly var translation = ref GetComponentData<Position>(owner).Value;
			ref var          velocity    = ref GetComponentData<Velocity>(owner).Value;
			if (!state.IsActiveOrChaining)
			{
				if (MathUtils.Distance(ability.StartPosition, translation.X) > 2.5f
				    && ability.ActiveTime > 0.1f)
				{
					velocity.X = (ability.StartPosition - translation.X) * 3;
				}

				ability.ActiveTime   = 0;
				ability.IsRetreating = false;
				return;
			}
			
			ref readonly var playState     = ref GetComponentData<UnitPlayState>(owner);
			ref readonly var unitDirection = ref GetComponentData<UnitDirection>(owner).Value;

			var wasRetreating = ability.IsRetreating;
			var retreatSpeed  = playState.MovementAttackSpeed * 3f;

			ability.IsRetreating =  ability.ActiveTime <= walkbackTime;
			ability.ActiveTime   += dt;

			if (!wasRetreating && ability.IsRetreating)
			{
				ability.StartPosition = translation.X;
				velocity.X            = -unitDirection * retreatSpeed;
			}

			// there is a little stop when the character is stopping retreating
			if (ability.ActiveTime >= DefaultRetreatAbility.StopTime && ability.ActiveTime <= walkbackTime)
			{
				// if he weight more, he will stop faster
				velocity.X = MathUtils.LerpNormalized(velocity.X, 0, playState.Weight * 0.25f * dt);
			}

			if (!ability.IsRetreating && ability.ActiveTime > walkbackTime)
			{
				// we add '2.8f' to boost the speed when backing up, so the unit can't chain retreat to go further
				if (wasRetreating)
					ability.BackVelocity = Math.Abs(ability.StartPosition - translation.X) * 2.8f;

				var newPosX = MathUtils.MoveTowards(translation.X, ability.StartPosition, ability.BackVelocity * dt);
				velocity.X = (newPosX - translation.X) / dt;
			}

			ref var unitController = ref GetComponentData<UnitControllerState>(owner);
			unitController.ControlOverVelocityX = true;
		}
	}
}