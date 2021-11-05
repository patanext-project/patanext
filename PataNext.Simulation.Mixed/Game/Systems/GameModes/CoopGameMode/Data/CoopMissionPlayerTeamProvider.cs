using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.GameModes.DataCoopMission
{
	public class CoopMissionPlayerTeamProvider : PlayerTeamProvider
	{
		public CoopMissionPlayerTeamProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);

			entityComponents.AddRange(new []
			{
				AsComponentType<SimulationAuthority>(),
				AsComponentType<ProtagonistTeamTag>()
			});
		}
	}
}