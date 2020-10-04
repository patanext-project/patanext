using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameModes
{
	public struct GameModeRequestEndRound : IComponentData
	{
	}

	public class MissionGameModeBase<TGameMode> : GameModeSystemBase<TGameMode>
		where TGameMode : struct, IComponentData
	{
		public MissionGameModeBase(WorldCollection collection) : base(collection)
		{
		}

		protected EntityQuery EndRoundQuery { get; set; }

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			EndRoundQuery = CreateEntityQuery(new[] {typeof(GameModeRequestEndRound)});
		}

		protected override async Task GetStateMachine(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				EndRoundQuery.RemoveAllEntities();
				while (!token.IsCancellationRequested
				       && !EndRoundQuery.Any())
				{
					await GameModePlayLoop();
					await Task.Yield();
				}

				await Task.Yield();
			}
		}

		protected virtual Task GameModePlayLoop()
		{
			return Task.CompletedTask;
		}
	}
}