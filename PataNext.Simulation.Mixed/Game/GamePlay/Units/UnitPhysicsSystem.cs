using System;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.InterTick;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	public class UnitPhysicsSystem : GameAppSystem, IUpdateSimulationPass
	{
		private IManagedWorldTime worldTime;

		public UnitPhysicsSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery unitQuery;

		public void OnSimulationUpdate()
		{
			var dt      = (float) worldTime.Delta.TotalSeconds;
			var gravity = new Vector3(0, -26f, 0);
			foreach (var entity in (unitQuery ??= CreateEntityQuery(new[]
			{
				AsComponentType<UnitControllerState>(),
				AsComponentType<GroundState>(),
				AsComponentType<Position>(),
				AsComponentType<Velocity>(),
				AsComponentType<UnitPlayState>(),
			})).GetEntities())
			{
				ref var          controllerState = ref GetComponentData<UnitControllerState>(entity);
				ref var          groundState     = ref GetComponentData<GroundState>(entity).Value;
				ref var          translation     = ref GetComponentData<Position>(entity).Value;
				ref var          velocity        = ref GetComponentData<Velocity>(entity).Value;
				ref readonly var unitPlayState   = ref GetComponentData<UnitPlayState>(entity);

				if (velocity.Y > 0)
					groundState = false;

				var previousPosition = translation;
				var target = controllerState.OverrideTargetPosition || !TryGetComponentData<Relative<UnitTargetDescription>>(entity, out var relativeTarget)
					? controllerState.TargetPosition
					: GetComponentData<Position>(relativeTarget.Target).Value.X;

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
						var acceleration = Math.Clamp(MathHelper.RcpSafe(unitPlayState.Weight), 0, 1) * 10;
						acceleration = Math.Min(acceleration * dt, 1) * 0.75f;

						velocity.X = MathHelper.LerpNormalized(velocity.X, 0, acceleration);
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