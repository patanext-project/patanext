using System;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.CoreMissions.Server
{
	public abstract class MissionScenarScript : ScenarScript
	{
		private PlayerTeamProvider teamProvider;
		
		public MissionScenarScript(WorldCollection wc) : base(wc)
		{
			DependencyResolver.Add(() => ref teamProvider);
		}

		protected GameEntity ProtagonistTeam;
		protected GameEntity EnemyTeam;

		protected override Task OnStart()
		{
			if (!GameWorld.TryGetSingleton<ProtagonistTeamTag>(out GameEntityHandle protagonistTeamHandle))
				throw new InvalidOperationException("No protagonist team found");

			EnemyTeam = Safe(teamProvider.SpawnEntityWithArguments(new()));
			AddComponent(EnemyTeam, new SimulationAuthority());
			GetBuffer<TeamEnemies>(EnemyTeam).Add(new(Safe(protagonistTeamHandle)));
			GetBuffer<TeamEnemies>(protagonistTeamHandle).Add(new(EnemyTeam));

			GameWorld.Link(EnemyTeam.Handle, Self.Handle, true);

			ProtagonistTeam = Safe(protagonistTeamHandle);

			return Task.CompletedTask;
		}
	}
}