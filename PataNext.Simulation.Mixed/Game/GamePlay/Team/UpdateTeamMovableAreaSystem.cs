using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Team
{
	public class UpdateTeamMovableAreaSystem : GameAppSystem
	{
		public UpdateTeamMovableAreaSystem(WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery teamAreaQuery;
		private EntityQuery contributionQuery;

		protected override void OnUpdate()
		{
			var movableAreaAccessor = GetAccessor<TeamMovableArea>();
			foreach (var entity in (teamAreaQuery ??= CreateEntityQuery(new[]
			{
				typeof(TeamMovableArea),
				typeof(TeamDescription),
				typeof(IsSimulationOwned)
			})))
			{
				movableAreaAccessor[entity] = new TeamMovableArea {Left = float.PositiveInfinity, Right = float.NegativeInfinity};
			}

			var teamRelativeAccessor = GetAccessor<Relative<TeamDescription>>();
			var positionAccessor     = GetAccessor<Position>();
			var contributionAccessor = GetAccessor<ContributeToTeamMovableArea>();
			foreach (var entity in (contributionQuery ??= CreateEntityQuery(new[]
			{
				typeof(ContributeToTeamMovableArea),
				typeof(Position),
				typeof(Relative<TeamDescription>),
				typeof(IsSimulationOwned)
			})))
			{
				ref readonly var teamRelative = ref teamRelativeAccessor[entity];
				if (!HasComponent<TeamMovableArea>(teamRelative.Target))
					continue;

				ref var area = ref movableAreaAccessor[teamRelative.Target];

				ref readonly var position   = ref positionAccessor[entity];
				ref readonly var contribute = ref contributionAccessor[entity];
				if (!area.IsValid)
				{
					area.IsValid = true;
					area.Left    = position.Value.X - contribute.Size - contribute.Center;
					area.Right   = position.Value.X + contribute.Size + contribute.Center;

					continue;
				}

				area.Left  = MathF.Min(position.Value.X - contribute.Size - contribute.Center, area.Left);
				area.Right = MathF.Max(position.Value.X + contribute.Size + contribute.Center, area.Right);
			}
		}
	}
}