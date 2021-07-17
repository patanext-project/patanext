using System;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server
{
	public enum ENearestOrder
	{
		SelfThenRelative,
		RelativeThenSelf,
		SelfOnly,
		RelativeOnly
	}

	public abstract class ScriptBase<TProvider> : AbilityScriptModule<TProvider>
		where TProvider : AppObject, IRuntimeAbilityProvider
	{
		public (GameEntity enemy, float dist, bool isSelf) GetNearestEnemyComplete(GameEntityHandle entity,
		                                                                           float?           maxSelfDistance, float? maxRelativeDistance,
		                                                                           ENearestOrder    nearestOrder = ENearestOrder.SelfThenRelative)
		{
			if (!GameWorld.Contains(entity))
				return default;

			var defaultSeekingRange = GetComponentData<UnitPlayState>(entity).AttackSeekRange;
			maxSelfDistance     ??= defaultSeekingRange;
			maxRelativeDistance ??= defaultSeekingRange;

			Relative<UnitTargetDescription> relative;
			UnitEnemySeekingState           seekingState;

			var result = (enemy: default(GameEntity), dist: 0f);
			switch (nearestOrder)
			{
				case ENearestOrder.SelfThenRelative:
					if (TryGetComponentData(entity, out seekingState))
					{
						result = (seekingState.Enemy, seekingState.SelfDistance);
						if (result.dist <= maxSelfDistance)
							return (result.enemy, result.dist, true);

						result = (seekingState.Enemy, seekingState.RelativeDistance);
						if (result.dist <= maxRelativeDistance)
							return (result.enemy, result.dist, false);
					}

					if (TryGetComponentData(entity, out relative)
					    && TryGetComponentData(relative.Handle, out seekingState))
					{
						result = (seekingState.Enemy, seekingState.SelfDistance);
						if (result.dist <= maxRelativeDistance)
							return (result.enemy, result.dist, false);
					}

					return default;
				case ENearestOrder.RelativeThenSelf:
					if (TryGetComponentData(entity, out seekingState))
					{
						result = (seekingState.Enemy, seekingState.RelativeDistance);
						if (result.dist <= maxRelativeDistance)
							return (result.enemy, result.dist, false);
						
						result = (seekingState.Enemy, seekingState.SelfDistance);
						if (result.dist <= maxSelfDistance)
							return (result.enemy, result.dist, true);
					}

					if (TryGetComponentData(entity, out relative)
					    && TryGetComponentData(relative.Handle, out seekingState))
					{
						result = (seekingState.Enemy, seekingState.SelfDistance);
						if (result.dist <= maxRelativeDistance)
							return (result.enemy, result.dist, false);
					}

					return default;
				case ENearestOrder.SelfOnly:
					if (TryGetComponentData(entity, out seekingState))
					{
						return (seekingState.Enemy, seekingState.SelfDistance, false);
					}

					break;
				case ENearestOrder.RelativeOnly:
					if (TryGetComponentData(entity, out relative)
					    && TryGetComponentData(entity, out seekingState))
					{
						return (seekingState.Enemy, seekingState.RelativeDistance, false);
					}

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(nearestOrder), nearestOrder, null);
			}

			return default;
		}

		public (GameEntity enemy, float dist) GetNearestEnemy(GameEntityHandle entity,
		                                                      float?           maxSelfDistance, float? maxRelativeDistance,
		                                                      ENearestOrder    nearestOrder = ENearestOrder.SelfThenRelative)
		{
			var (enemy, dist, _) = GetNearestEnemyComplete(entity,
				maxSelfDistance, maxRelativeDistance,
				nearestOrder);
			return (enemy, dist);
		}

		public struct RoutineGetNearOfEnemyResult
		{
			public bool       CanTriggerAttack;
			public float      Distance;
			public Vector3    Target;
			public GameEntity Enemy;
			public Vector3    EnemyPosition;
		}

		public RoutineGetNearOfEnemyResult RoutineGetNearOfEnemy(GameEntity    entity,
		                                                         Vector2       deltaPosition,
		                                                         float? attackNearDistance = null,
		                                                         float? attackFarDistance = null,
		                                                         float?        selfDistance      = null, float? targetDistance = null,
		                                                         ENearestOrder nearestOrder      = ENearestOrder.SelfThenRelative,
		                                                         bool          addEnemyWeakPoint = true)
		{
			var hasRelative = HasComponent<Relative<UnitTargetDescription>>(entity);
			// If the unit doesn't have a playstate (can happen if it's fixed to a position)
			// Then modify the distance based on the SeekRange and force nearestOrder to be SelfOnly
			if (!hasRelative && TryGetComponentData(entity, out UnitPlayState playState))
			{
				targetDistance = playState.AttackSeekRange;
				selfDistance   = playState.AttackSeekRange;

				nearestOrder = ENearestOrder.SelfOnly;
			}
		
			var (enemy, enemyDist, isFromSelf) = GetNearestEnemyComplete(entity.Handle, selfDistance, targetDistance, nearestOrder);
			Console.WriteLine($"{enemy} {enemyDist} {isFromSelf}");
			if (enemy != default)
			{
				RoutineGetNearOfEnemyResult result;
				
				var position       = GetComponentData<Position>(entity).Value;
				var targetPosition = GetComponentData<Position>(enemy).Value;

				result.EnemyPosition = targetPosition;
				result.Enemy         = enemy;

				// Search for any weakpoint the enemy has, and if it does, add it to the deltaPosition var
				if (addEnemyWeakPoint)
				{
					if (TryGetComponentBuffer<UnitWeakPoint>(enemy, out var weakPoints)
					    && weakPoints.GetNearest(targetPosition - position) is var (weakPoint, dist) && dist >= 0)
						deltaPosition += weakPoint.XY();
				}

				targetPosition.X -= deltaPosition.X;

				result.Target   = targetPosition;
				result.Distance = enemyDist;

				// If we're near enough of where we should throw the spear, throw it.
				attackNearDistance ??= selfDistance ?? 2;
				attackFarDistance  ??= selfDistance ?? 2;

				// (T = target, Y = Your unit, E = Enemy, . = 1m)
				//
				//	Near=2 Far=2
				//
				//  . . . . . . .
				//    T     Y E
				//    2 |   5 6
				//
				//  [Will not work since we're way too near of the enemy]
				//

				var finalDist = targetPosition.X - position.X;
				if (GetComponentData<UnitDirection>(entity.Handle).IsRight)
					finalDist = -finalDist;
				
				result.CanTriggerAttack = attackNearDistance >= finalDist && finalDist >= -attackFarDistance; // the last minus is important

				return result;
			}

			return default;
		}

		protected ScriptBase(WorldCollection collection) : base(collection)
		{
		}
	}
}