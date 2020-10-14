using System;
using System.Numerics;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.GamePlay.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Abilities
{
	public struct AbilityControlVelocity : IComponentData
	{
		public bool IsActive;

		public Boolean3 Keep;
		public Boolean3 Control;

		public bool    TargetFromCursor;
		public Vector3 TargetPosition;
		public float   OffsetFactor;
		public float   Acceleration;

		public bool  HasCustomMovementSpeed;
		public float CustomMovementSpeed;

		public void ResetPositionX(float acceleration, float offsetFactor = 1)
		{
			SetTargetPositionX(0, acceleration, offsetFactor);
		}

		public void SetTargetPositionX(float position, float acceleration, float offsetFactor = 1)
		{
			Keep    = default;
			Control = default;

			IsActive         = true;
			TargetFromCursor = true;
			TargetPosition.X = position;
			OffsetFactor     = offsetFactor;
			Acceleration     = acceleration;
		}

		public void StayAtCurrentPositionX(float acceleration)
		{
			Control = default;
			
			IsActive     = true;
			Keep.X       = true;
			Acceleration = acceleration;
		}
	}

	[UpdateBefore(typeof(UnitPhysicsSystem))]
	public class AbilityControlVelocitySystem : GameAppSystem, IPostUpdateSimulationPass
	{
		private IManagedWorldTime worldTime;

		public AbilityControlVelocitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery entityQuery;

		public void OnAfterSimulationUpdate()
		{
			var dt = (float) worldTime.Delta.TotalSeconds;

			var targetAccessor = new ComponentDataAccessor<AbilityControlVelocity>(GameWorld);
			var ownerAccessor  = new ComponentDataAccessor<Owner>(GameWorld);

			var positionComponentType = AsComponentType<Position>();
			var positionAccessor      = new ComponentDataAccessor<Position>(GameWorld);
			var playStateAccessor     = new ComponentDataAccessor<UnitPlayState>(GameWorld);
			var velocityAccessor      = new ComponentDataAccessor<Velocity>(GameWorld);
			var controllerAccessor    = new ComponentDataAccessor<UnitControllerState>(GameWorld);

			var cursorComponentType = AsComponentType<Relative<UnitTargetDescription>>();
			var cursorAccessor      = new ComponentDataAccessor<Relative<UnitTargetDescription>>(GameWorld);

			foreach (var entity in (entityQuery ??= CreateEntityQuery(new[]
			{
				AsComponentType<AbilityControlVelocity>(),
				AsComponentType<Owner>()
			})).GetEntities())
			{
				ref var target = ref targetAccessor[entity];
				if (!target.IsActive)
					continue;
				target.IsActive = false;

				Console.WriteLine("?");

				ref readonly var owner = ref ownerAccessor[entity].Target;

				ref var position   = ref positionAccessor[owner].Value;
				ref var playState  = ref playStateAccessor[owner];
				ref var velocity   = ref velocityAccessor[owner].Value;
				ref var controller = ref controllerAccessor[owner];

				if (target.HasCustomMovementSpeed)
					playState.MovementAttackSpeed = target.CustomMovementSpeed;

				if (target.Keep.X)
				{
					if (target.Acceleration.Equals(float.PositiveInfinity))
						velocity.X = 0;
					else if (target.Acceleration > 0)
						velocity.X = MathUtils.LerpNormalized(velocity.X, 0, target.Acceleration * dt);
				}
				else
				{
					var targetPosition = target.TargetPosition;
					if (target.TargetFromCursor && HasComponent(owner, cursorComponentType))
					{
						if (HasComponent(cursorAccessor[owner].Target, positionComponentType))
							targetPosition += positionAccessor[cursorAccessor[owner].Target].Value;
					}

					if (MathF.Abs(target.OffsetFactor) > 0.01f && TryGetComponentData(owner, out UnitTargetOffset offset))
					{
						targetPosition.X += offset.Value * target.OffsetFactor;
					}

					velocity.X = AbilityUtility.GetTargetVelocityX(new AbilityUtility.GetTargetVelocityParameters
					{
						TargetPosition   = targetPosition,
						PreviousPosition = new Vector3(position.X, 0, 0),
						PreviousVelocity = velocity,
						PlayState        = playState,
						Acceleration     = target.Acceleration,
						Delta            = dt
					}, 0, 0.5f);

					Console.WriteLine("yes");
				}

				controller.ControlOverVelocityX = true;
			}
		}
	}
}