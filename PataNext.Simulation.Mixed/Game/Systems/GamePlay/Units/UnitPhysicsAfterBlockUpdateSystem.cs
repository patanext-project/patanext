using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	[UpdateAfter(typeof(UnitCollisionSystem))]
	public class UnitPhysicsAfterBlockUpdateSystem : GameAppSystem, IPostUpdateSimulationPass
	{
		public UnitPhysicsAfterBlockUpdateSystem(WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery unitQuery;

		public void OnAfterSimulationUpdate()
		{
			for (var iter = 0; iter < 2; iter++)
			{
				var controllerStateAccessor = GetAccessor<UnitControllerState>();
				var unitDirectionAccessor   = GetAccessor<UnitDirection>();
				var positionAccessor        = GetAccessor<Position>();
				var contributeAccessor      = GetAccessor<ContributeToTeamMovableArea>();
				var teamRelativeAccessor    = GetAccessor<Relative<TeamDescription>>();
				foreach (var entity in unitQuery ??= CreateEntityQuery(new[]
				{
					typeof(UnitDescription),
					typeof(UnitControllerState),
					typeof(UnitDirection),
					typeof(Position),
					typeof(ContributeToTeamMovableArea),
					typeof(Relative<TeamDescription>),
					typeof(SimulationAuthority)
				}, new[]
				{
					typeof(LivableIsDead)
				}))
				{
					ref readonly var controllerState = ref controllerStateAccessor[entity];
					ref readonly var relativeTeam    = ref teamRelativeAccessor[entity];
					ref readonly var unitDirection   = ref unitDirectionAccessor[entity];

					ref var position   = ref positionAccessor[entity];
					ref var contribute = ref contributeAccessor[entity];

					var previousTranslation = position.Value;
					if (!controllerState.PassThroughEnemies && TryGetComponentBuffer<TeamEnemies>(relativeTeam.Handle, out var enemies))
					{
						for (var i = 0; i != enemies.Count; i++)
						{
							if (!TryGetComponentData(enemies[i].Team.Handle, out TeamMovableArea enemyArea))
								continue;

							// If the new position is superior the area and the previous one inferior, teleport back to the area.
							var size                                                                                = contribute.Size * 0.5f + contribute.Center;
							if (position.Value.X + size > enemyArea.Left && unitDirection.IsRight) position.Value.X = enemyArea.Left - size;

							if (position.Value.X - size < enemyArea.Right && unitDirection.IsLeft) position.Value.X = enemyArea.Right + size;
							
							// if it's inside...
							if (position.Value.X + size > enemyArea.Left && position.Value.X - size < enemyArea.Right)
							{
								if (unitDirection.IsLeft)
									position.Value.X = enemyArea.Right + size;
								else if (unitDirection.IsRight)
									position.Value.X = enemyArea.Left - size;
							}
						}
					}

					position.Value.Y = previousTranslation.Y;
					position.Value.Z = previousTranslation.Z;

					if (HasComponent<TeamMovableArea>(relativeTeam.Handle) 
					    && HasComponent<SimulationAuthority>(relativeTeam.Handle))
					{
						ref var teamArea = ref GetComponentData<TeamMovableArea>(relativeTeam.Handle);

						teamArea.Left  = Math.Min(position.Value.X - contribute.Size - contribute.Center, teamArea.Left);
						teamArea.Right = Math.Max(position.Value.X + contribute.Size + contribute.Center, teamArea.Right);
					}

					for (var v = 0; v != 3; v++)
						position.Value.Ref(v) = float.IsNaN(position.Value.Ref(v)) ? 0.0f : position.Value.Ref(v);
				}
			}
		}
	}
}