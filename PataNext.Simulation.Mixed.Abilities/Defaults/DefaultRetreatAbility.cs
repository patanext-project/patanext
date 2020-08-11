using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Simulation.Mixed.Abilities.Subset;
using PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.Simulation.Mixed.Abilities.Defaults
{
	public struct DefaultRetreatAbility : IComponentData
	{
		public const float StopTime      = 1.5f;
		public const float MaxActiveTime = StopTime + 0.5f;

		public int LastActiveId;

		public float AccelerationFactor;
		public float StartPosition;
		public float BackVelocity;
		public bool  IsRetreating;
		public float ActiveTime;

		public class Register : RegisterGameHostComponentData<DefaultRetreatAbility>
		{
		}
	}

	public class DefaultRetreatAbilityProvider : BaseRhythmAbilityProvider<DefaultRetreatAbility>
	{
		public DefaultRetreatAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "retreat";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<RetreatCommand>();
		}

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);
			GameWorld.GetComponentData<DefaultRetreatAbility>(entity) = new DefaultRetreatAbility
			{
				AccelerationFactor = 1
			};
		}
	}

	public class DefaultRetreatAbilitySystem : BaseAbilitySystem
	{
		private IManagedWorldTime worldTime;

		public DefaultRetreatAbilitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery abilityQuery;

		public override void OnAbilityPreSimulationPass()
		{
			var dt = (float) worldTime.Delta.TotalSeconds;

			foreach (var entity in (abilityQuery ??= CreateEntityQuery(new[]
			{
				typeof(DefaultRetreatAbility),
				typeof(AbilityState)
			})).GetEntities())
			{
				ref var          ability = ref GetComponentData<DefaultRetreatAbility>(entity);
				ref readonly var state   = ref GetComponentData<AbilityState>(entity);
				ref readonly var owner   = ref GetComponentData<Owner>(entity).Target;

				if (state.ActivationVersion != ability.LastActiveId)
				{
					ability.IsRetreating = false;
					ability.ActiveTime   = 0;
					ability.LastActiveId = state.ActivationVersion;
				}

				ref readonly var translation = ref GetComponentData<Position>(owner).Value;

				ref var velocity = ref GetComponentData<Velocity>(owner).Value;
				if (!state.IsActiveOrChaining)
				{
					if (MathHelper.Distance(ability.StartPosition, translation.X) > 2.5f
					    && ability.ActiveTime > 0.1f)
					{
						velocity.X = (ability.StartPosition - translation.X) * 3;
					}

					ability.ActiveTime   = 0;
					ability.IsRetreating = false;
					return;
				}

				const float walkbackTime = 3.25f;

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
					// if he weight more, he will stop faster
					velocity.X = MathHelper.LerpNormalized(velocity.X, 0, playState.Weight * 0.25f * dt);

				if (!ability.IsRetreating && ability.ActiveTime > walkbackTime)
				{
					// we add '2.8f' to boost the speed when backing up, so the unit can't chain retreat to go further
					if (wasRetreating)
						ability.BackVelocity = Math.Abs(ability.StartPosition - translation.X) * 2.8f;

					var newPosX = MathHelper.MoveTowards(translation.X, ability.StartPosition, ability.BackVelocity * dt);
					velocity.X = (newPosX - translation.X) / dt;
				}

				ref var unitController = ref GetComponentData<UnitControllerState>(owner);
				unitController.ControlOverVelocityX = true;
			}
		}
	}
}