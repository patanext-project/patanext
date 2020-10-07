using System.Collections.Specialized;
using System.Numerics;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components;
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

		public Boolean3 Control;

		public bool    TargetFromCursor;
		public Vector3 TargetPosition;
		public float   Acceleration;

		public bool  HasCustomMovementSpeed;
		public float CustomMovementSpeed;
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

				ref readonly var owner = ref ownerAccessor[entity].Target;

				ref var position   = ref positionAccessor[owner].Value;
				ref var playState  = ref playStateAccessor[owner];
				ref var velocity   = ref velocityAccessor[owner].Value;
				ref var controller = ref controllerAccessor[owner];

				if (target.HasCustomMovementSpeed)
					playState.MovementAttackSpeed = target.CustomMovementSpeed;

				var targetPosition = target.TargetPosition;
				if (target.TargetFromCursor && HasComponent(owner, cursorComponentType))
				{
					if (HasComponent(cursorAccessor[owner].Target, positionComponentType))
						targetPosition += positionAccessor[cursorAccessor[owner].Target].Value;
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
				controller.ControlOverVelocityX = true;
			}
		}
	}
}