using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Transform.Components;
using static StormiumTeam.GameBase.MathUtils;

namespace PataNext.Simulation.Mixed.Abilities.Subset
{
	/// <summary>
	/// Represent a subset of the march ability that can be used on other abilities.
	/// </summary>
	public struct DefaultSubsetMarch : IComponentData
	{
		/// <summary>
		/// Which Movable target should we move?
		/// </summary>
		[Flags]
		public enum ETarget
		{
			None     = 0,
			Cursor   = 1 << 1,
			Movement = 1 << 2,
			All      = Cursor | Movement
		}

		/// <summary>
		/// Is the subset component active?
		/// </summary>
		public bool IsActive;

		/// <summary>
		/// What is our current movable target?
		/// </summary>
		public ETarget Target;

		/// <summary>
		/// The acceleration when marching
		/// </summary>
		public float AccelerationFactor;

		/// <summary>
		/// How much time was this subset active?
		/// </summary>
		/// <remarks>
		///	This variable was named 'Delta'
		/// </remarks>
		public float ActiveTime;
		
		public class Register : RegisterGameHostComponentData<DefaultSubsetMarch>
		{}
	}

	public class DefaultSubsetMarchAbilitySystem : BaseAbilitySystem
	{
		private IManagedWorldTime worldTime;

		public DefaultSubsetMarchAbilitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery abilityQuery;
		private EntityQuery validOwnerQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			abilityQuery = CreateEntityQuery(new[]
			{
				typeof(DefaultSubsetMarch),
				typeof(Owner)
			});
			validOwnerQuery = CreateEntityQuery(new[]
			{
				typeof(Position),
				typeof(Velocity),
				typeof(UnitPlayState),
				typeof(UnitControllerState),
				typeof(UnitTargetOffset),
				typeof(UnitDirection),
				typeof(Relative<UnitTargetDescription>)
			});
		}

		public override void OnAbilityUpdate()
		{
			validOwnerQuery.CheckForNewArchetypes();

			var timeDelta = (float) worldTime.Delta.TotalSeconds;
			foreach (var entity in abilityQuery.GetEnumerator())
			{
				ref readonly var owner = ref GetComponentData<Owner>(entity).Target;
				if (!validOwnerQuery.MatchAgainst(owner))
					continue;

				ref var subSet = ref GetComponentData<DefaultSubsetMarch>(entity);
				if (!subSet.IsActive)
				{
					subSet.ActiveTime = 0;
					continue;
				}

				ref readonly var unitTargetRelative = ref GetComponentData<Relative<UnitTargetDescription>>(owner).Target;
				ref readonly var unitPlayState      = ref GetComponentData<UnitPlayState>(owner);

				subSet.ActiveTime += timeDelta;

				ref var targetPosition = ref GetComponentData<Position>(unitTargetRelative).Value.X;
				ref var ownerPosition  = ref GetComponentData<Position>(owner).Value.X;

				ref readonly var targetOffset = ref GetComponentData<UnitTargetOffset>(owner).Value;

				float acceleration, walkSpeed;
				int   direction;

				// Cursor movement
				if ((subSet.Target & DefaultSubsetMarch.ETarget.Cursor) != 0
				    && GameWorld.HasComponent<UnitTargetControlTag>(owner)
				    && subSet.ActiveTime <= 3.75f)
				{
					direction = GetComponentData<UnitDirection>(owner).Value;

					// a different acceleration (not using the unit weight)
					acceleration = subSet.AccelerationFactor;
					acceleration = Math.Min(acceleration * timeDelta, 1);

					walkSpeed      =  unitPlayState.MovementSpeed;
					targetPosition += walkSpeed * direction * (subSet.ActiveTime > 0.25f ? 1 : LerpNormalized(2, 1, subSet.ActiveTime + 0.25f)) * acceleration;
				}

				// Character movement
				if ((subSet.Target & DefaultSubsetMarch.ETarget.Movement) != 0)
				{
					// if (!groundState.Value)
					// 	continue;

					ref var velocity = ref GetComponentData<Velocity>(owner).Value;

					// to not make tanks op, we need to get the weight from entity and use it as an acceleration factor
					// We need to get the abs of the AccelerationFactor since the backward ability use -1
					acceleration = Math.Clamp(RcpSafe(unitPlayState.Weight), 0, 1) * Math.Abs(subSet.AccelerationFactor) * 50;
					acceleration = Math.Min(acceleration * timeDelta, 1);

					walkSpeed = unitPlayState.MovementSpeed;
					direction = Math.Sign(targetPosition + targetOffset - ownerPosition);

					velocity.X = LerpNormalized(velocity.X, walkSpeed * direction, acceleration);

					ref var controllerState = ref GetComponentData<UnitControllerState>(owner);
					controllerState.ControlOverVelocityX = true;
				}
			}
		}
	}
}