using System;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	public class UnitPhysicsSystem : GameAppSystem, IPostUpdateSimulationPass
	{
		private IManagedWorldTime worldTime;

		public UnitPhysicsSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery unitQuery;

		public void OnAfterSimulationUpdate()
		{
			var dt      = (float) worldTime.Delta.TotalSeconds;
			var gravity = new Vector3(0, -26f, 0);

			var controllerStateAccessor = new ComponentDataAccessor<UnitControllerState>(GameWorld);
			var groundStateAccessor     = new ComponentDataAccessor<GroundState>(GameWorld);
			var positionAccessor        = new ComponentDataAccessor<Position>(GameWorld);
			var velocityAccessor        = new ComponentDataAccessor<Velocity>(GameWorld);
			var playStateAccessor       = new ComponentDataAccessor<UnitPlayState>(GameWorld);
			foreach (var entity in (unitQuery ??= CreateEntityQuery(new[]
			{
				AsComponentType<UnitControllerState>(),
				AsComponentType<GroundState>(),
				AsComponentType<Position>(),
				AsComponentType<Velocity>(),
				AsComponentType<UnitPlayState>(),
			})))
			{
				ref var          controllerState = ref controllerStateAccessor[entity];
				ref var          groundState     = ref groundStateAccessor[entity].Value;
				ref var          translation     = ref positionAccessor[entity].Value;
				ref var          velocity        = ref velocityAccessor[entity].Value;
				ref readonly var unitPlayState   = ref playStateAccessor[entity];

				if (velocity.Y > 0)
					groundState = false;

				var previousPosition = translation;
				var target = controllerState.OverrideTargetPosition || !TryGetComponentData<Relative<UnitTargetDescription>>(entity, out var relativeTarget)
					? controllerState.TargetPosition
					: GetComponentData<Position>(relativeTarget.Target).Value.X + GetComponentData<UnitTargetOffset>(entity).Value;

				// TODO: Livable state
				/*if (livableHealthFromEntity.Exists(entity) && livableHealthFromEntity[entity].IsDead)
				{
					controllerState.ControlOverVelocity.x = true;
					if (groundState.Value)
						velocity.Value.x = math.lerp(velocity.Value.x, 0, 2.5f * dt);
				}*/

				if (!controllerState.ControlOverVelocityX)
				{
					// todo: find a good way for client to predict that nicely
					if (groundState)
					{
						var ps = unitPlayState;
						ps.MovementAttackSpeed = ps.MovementReturnSpeed;
						velocity.X = AbilityUtility.GetTargetVelocityX(new AbilityUtility.GetTargetVelocityParameters
						{
							TargetPosition   = new Vector3(target, 0, 0),
							PreviousPosition = translation,
							PreviousVelocity = velocity,
							PlayState        = ps,
							Acceleration     = 10,
							Delta            = dt
						}, deaccel_distance: 0, deaccel_distance_max: 0.25f);
					}
					else
					{
						var acceleration = Math.Clamp(MathUtils.RcpSafe(unitPlayState.Weight), 0, 1) * 10;
						acceleration = Math.Min(acceleration * dt, 1) * 0.75f;

						velocity.X = MathUtils.LerpNormalized(velocity.X, 0, acceleration);
					}
				}

				if (!controllerState.ControlOverVelocityY)
					if (!groundState)
						velocity += gravity * dt;

				for (var i = 0; i < 3; i++)
					velocity.Ref(i) = float.IsNaN(velocity.Ref(i)) ? 0.0f : velocity.Ref(i);

				translation += velocity * dt;
				if (translation.Y < 0) // meh
					translation.Y = 0;

				groundState = translation.Y <= 0;
				if (!controllerState.ControlOverVelocityY && groundState)
					velocity.Y = Math.Max(velocity.Y, 0);

				for (var i = 0; i < 3; i++)
					translation.Ref(i) = float.IsNaN(translation.Ref(i)) ? 0.0f : translation.Ref(i);

				controllerState.ControlOverVelocity    = default;
				controllerState.OverrideTargetPosition = false;
				controllerState.PassThroughEnemies     = false;
				controllerState.PreviousPosition       = previousPosition;
			}
		}
	}
}