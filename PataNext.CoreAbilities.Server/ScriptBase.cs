using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase.Roles.Components;

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
		public (GameEntity enemy, float dist) GetNearestEnemy(GameEntity entity, float? maxSelfDistance, float? maxRelativeDistance, ENearestOrder nearestOrder = ENearestOrder.SelfThenRelative)
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
						result = (seekingState.Enemy, seekingState.Distance);
						if (result.dist <= maxSelfDistance)
							return result;
					}

					if (TryGetComponentData(entity, out relative)
					    && TryGetComponentData(relative.Target, out seekingState))
					{
						result = (seekingState.Enemy, seekingState.Distance);
						if (result.dist <= maxRelativeDistance)
							return result;
					}

					return default;
				case ENearestOrder.RelativeThenSelf:
					if (TryGetComponentData(entity, out relative)
					    && TryGetComponentData(relative.Target, out seekingState))
					{
						result = (seekingState.Enemy, seekingState.Distance);
						if (result.dist <= maxRelativeDistance)
							return result;
					}

					if (TryGetComponentData(entity, out seekingState))
					{
						result = (seekingState.Enemy, seekingState.Distance);
						if (result.dist <= maxSelfDistance)
							return result;
					}

					return default;
				case ENearestOrder.SelfOnly:
					if (TryGetComponentData(entity, out seekingState))
					{
						return (seekingState.Enemy, seekingState.Distance);
					}

					break;
				case ENearestOrder.RelativeOnly:
					if (TryGetComponentData(entity, out relative)
					    && TryGetComponentData(entity, out seekingState))
					{
						return (seekingState.Enemy, seekingState.Distance);
					}

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(nearestOrder), nearestOrder, null);
			}

			return default;
		}

		protected ScriptBase(WorldCollection collection) : base(collection)
		{
		}
	}
}