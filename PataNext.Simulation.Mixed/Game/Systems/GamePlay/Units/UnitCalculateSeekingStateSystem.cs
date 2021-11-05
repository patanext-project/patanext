using System;
using System.Collections.Generic;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	public class UnitCalculateSeekingStateSystem : GameAppSystem, IUpdateSimulationPass
	{
		private PooledList<GameEntityHandle> linkedEntitiesToProcess = new();

		public UnitCalculateSeekingStateSystem(WorldCollection collection) : base(collection)
		{
			AddDisposable(linkedEntitiesToProcess);
		}

		private EntityQuery teamQuery;

		private EntityQuery freeSeekerMask;
		private EntityQuery linkedSeekerMask;
		private EntityQuery relativeTargetMask;
		
		private EntityQuery enemyMask;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			freeSeekerMask = CreateEntityQuery(new[]
			{
				typeof(Position),
				typeof(UnitEnemySeekingState)
			}, none: new[]
			{
				typeof(Relative<UnitTargetDescription>)
			});
			linkedSeekerMask = CreateEntityQuery(new[]
			{
				typeof(Position),
				typeof(UnitEnemySeekingState),
				typeof(Relative<UnitTargetDescription>)
			}, none: new[]
			{
				typeof(UnitTargetDescription)
			});
			relativeTargetMask = CreateEntityQuery(new[]
			{
				typeof(Position),
				typeof(UnitEnemySeekingState)
			});

			// anything that has health and a position is an enemy
			enemyMask = CreateEntityQuery(new[]
			{
				typeof(Position),
				typeof(LivableHealth)
			}, none: new[]
			{
				typeof(UnitTargetDescription),
				typeof(LivableIsDead)
			});
		}

		public void OnSimulationUpdate()
		{
			freeSeekerMask.CheckForNewArchetypes();
			linkedSeekerMask.CheckForNewArchetypes();
			relativeTargetMask.CheckForNewArchetypes();
			enemyMask.CheckForNewArchetypes();

			var positionAccessor     = GetAccessor<Position>();
			var seekingStateAccessor = GetAccessor<UnitEnemySeekingState>();

			linkedEntitiesToProcess.Clear();

			foreach (var team in teamQuery ??= CreateEntityQuery(new[]
			{
				typeof(TeamDescription),
				typeof(TeamEnemies),
				typeof(TeamEntityContainer)
			}))
			{
				var entityBuffer = GetBuffer<TeamEntityContainer>(team).Reinterpret<GameEntity>();
				var enemyBuffer  = GetBuffer<TeamEnemies>(team).Reinterpret<GameEntity>();

				var entCount     = entityBuffer.Count;
				var enmTeamCount = enemyBuffer.Count;
				for (var ent = 0; ent < entCount; ent++)
				{
					var unit = entityBuffer[ent].Handle;
					if (linkedSeekerMask.MatchAgainst(unit))
					{
						var relative = GetComponentData<Relative<UnitTargetDescription>>(unit);
						if (GameWorld.Exists(relative.Target) && relativeTargetMask.MatchAgainst(relative.Handle))
						{
							linkedEntitiesToProcess.Add(unit);
							continue;
						}
					}

					if (!freeSeekerMask.MatchAgainst(unit)) 
						continue;
					
					ref readonly var pos   = ref positionAccessor[unit].Value.X;
					ref var          state = ref seekingStateAccessor[unit];

					state.Enemy    = default;
					state.SelfDistance = float.MaxValue;
					if (TryGetComponentData(unit, out UnitPlayState playState))
						state.SelfDistance = playState.AttackSeekRange;

					for (var enmTeam = 0; enmTeam < enmTeamCount; enmTeam++)
					{
						var enemyEntityBuffer = GetBuffer<TeamEntityContainer>(enemyBuffer[enmTeam].Handle).Reinterpret<GameEntity>();
						var enmEntCount       = enemyEntityBuffer.Count;
						for (var enmEnt = 0; enmEnt < enmEntCount; enmEnt++)
						{
							if (!enemyMask.MatchAgainst(enemyEntityBuffer[enmEnt].Handle))
								continue;

							var enemyPos = positionAccessor[enemyEntityBuffer[enmEnt].Handle].Value.X;
							var dist     = MathUtils.Distance(pos, enemyPos);
							if (dist < state.SelfDistance)
							{
								state.Enemy    = enemyEntityBuffer[enmEnt];
								state.SelfDistance = dist;
							}
						}
					}

					state.RelativeDistance = state.SelfDistance;
				}
			}

			var relativeAccessor = GetAccessor<Relative<UnitTargetDescription>>();
			foreach (var entity in linkedEntitiesToProcess)
			{
				ref readonly var relativeEntity       = ref relativeAccessor[entity].Target;
				ref readonly var relativeSeekingState = ref seekingStateAccessor[relativeEntity.Handle];

				ref readonly var position = ref positionAccessor[entity].Value;

				ref var seekingState = ref seekingStateAccessor[entity];

				var distance = float.MaxValue;
				if (TryGetComponentData(entity, out UnitPlayState playState))
					distance = playState.AttackSeekRange;

				seekingState = default;
				if (relativeSeekingState.SelfDistance <= distance && relativeSeekingState.Enemy != default)
				{
					seekingState              = relativeSeekingState;
					seekingState.SelfDistance = MathUtils.Distance(position.X, GetComponentData<Position>(relativeSeekingState.Enemy).Value.X);
				}
			}
		}
	}
}