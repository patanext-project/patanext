using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Structures;
using PataNext.Module.Simulation.Components.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Structures
{
	public class EndFlagUpdateSystem : GameAppSystem, IPostUpdateSimulationPass
	{
		private IScheduler scheduler;
		
		public EndFlagUpdateSystem([NotNull] WorldCollection collection) : base(collection)
		{
			scheduler = new Scheduler();
		}

		private EntityQuery teamFlagQuery;

		private EntityQuery validTeamQuery,
		                    validUnitQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			var baseQuery = CreateEntityQuery(new[]
			{
				typeof(EndFlagStructure),
				typeof(Position),
				typeof(UnitDirection)
			}, none: new []
			{
				typeof(EndFlagHasBeenPassed)
			});

			teamFlagQuery = QueryWith(baseQuery, new[] {typeof(EndFlagStructure.AllowedTeams)});

			validTeamQuery = CreateEntityQuery(new []
			{
				typeof(TeamEntityContainer)
			});
			validUnitQuery = CreateEntityQuery(new[]
			{
				typeof(Position)
			});
		}

		public void OnAfterSimulationUpdate()
		{
			validTeamQuery.CheckForNewArchetypes();
			validUnitQuery.CheckForNewArchetypes();

			var allowedTeamAccessor     = GetBufferAccessor<EndFlagStructure.AllowedTeams>();
			var entityContainerAccessor = GetBufferAccessor<TeamEntityContainer>();
			var positionAccessor        = GetAccessor<Position>();
			var directionAccessor       = GetAccessor<UnitDirection>();
			foreach (var flagHandle in teamFlagQuery)
			{
				var flagPosition  = positionAccessor[flagHandle].Value.X;
				var flagDirection = directionAccessor[flagHandle].Value;
				
				var allowedTeamBuffer = allowedTeamAccessor[flagHandle];
				foreach (var allowedTeam in allowedTeamBuffer)
				{
					var teamHandle = allowedTeam.Entity.Handle;
					if (!validTeamQuery.MatchAgainst(teamHandle))
						continue;
					
					foreach (var unitEntity in entityContainerAccessor[teamHandle])
					{
						var unitHandle   = unitEntity.Value.Handle;
						if (!validUnitQuery.MatchAgainst(unitHandle))
							continue;

						var unitPosition = positionAccessor[unitHandle].Value.X;
						if ((flagPosition - unitPosition) * flagDirection >= 0)
							continue;

						Console.WriteLine($"EndFlag triggered!");
						scheduler.Schedule(handle => AddComponent(handle, new EndFlagHasBeenPassed()), flagHandle, default);
					}
				}
			}
			
			scheduler.Run();
		}
	}
}