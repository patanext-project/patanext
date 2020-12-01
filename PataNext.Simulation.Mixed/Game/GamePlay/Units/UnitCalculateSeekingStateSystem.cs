using System.Collections.Generic;
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
		public UnitCalculateSeekingStateSystem(WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery teamQuery;
		private EntityQuery seekerMask;

		public void OnSimulationUpdate()
		{
			seekerMask ??= CreateEntityQuery(new[] {typeof(Position), typeof(UnitEnemySeekingState)});
			seekerMask.CheckForNewArchetypes();

			var positionAccessor     = GetAccessor<Position>();
			var seekingStateAccessor = GetAccessor<UnitEnemySeekingState>();

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
					if (!seekerMask.MatchAgainst(unit))
						continue;

					ref readonly var pos   = ref positionAccessor[unit].Value.X;
					ref var          state = ref seekingStateAccessor[unit];

					state.Enemy    = default;
					state.Distance = float.MaxValue;
					if ( /*!HasComponent<UnitTargetControlTag>(unit) && */TryGetComponentData(unit, out UnitPlayState playState))
						state.Distance = playState.AttackSeekRange;

					for (var enmTeam = 0; enmTeam < enmTeamCount; enmTeam++)
					{
						var enemyEntityBuffer = GetBuffer<TeamEntityContainer>(enemyBuffer[enmTeam].Handle).Reinterpret<GameEntity>();
						var enmEntCount       = enemyEntityBuffer.Count;
						for (var enmEnt = 0; enmEnt < enmEntCount; enmEnt++)
						{
							if (!HasComponent<LivableHealth>(enemyEntityBuffer[enmEnt].Handle))
								continue;

							var enemyPos = positionAccessor[enemyEntityBuffer[enmEnt].Handle].Value.X;
							var dist     = MathUtils.Distance(pos, enemyPos);
							if (dist < state.Distance)
							{
								state.Enemy    = enemyEntityBuffer[enmEnt];
								state.Distance = dist;
							}
						}
					}

					/*if (HasComponent<UnitTargetControlTag>(unit)
					    && TryGetComponentData(unit, out Relative<UnitTargetDescription> unitTarget)
					    && HasComponent<UnitEnemySeekingState>(unitTarget.Target))
					{
						seekingStateAccessor[unitTarget.Target] = state;
						if (TryGetComponentData(unit, out playState) && state.Distance > playState.AttackSeekRange)
							state = default;
					}*/
				}
			}
		}
	}
}