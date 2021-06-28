using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Providers
{
	public struct CreatePlayerTeam
	{
		public GameEntity OptionalClub;
	}

	public class PlayerTeamProvider : BaseProvider<CreatePlayerTeam>
	{
		public PlayerTeamProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				AsComponentType<TeamDescription>(),
				AsComponentType<TeamAllies>(),
				AsComponentType<TeamEnemies>(),
				AsComponentType<TeamEntityContainer>(),
				AsComponentType<TeamMovableArea>(),
			});
		}

		public override void SetEntityData(GameEntityHandle entity, CreatePlayerTeam data)
		{
			if (GameWorld.Exists(data.OptionalClub))
			{
				Console.WriteLine(data.OptionalClub);
				if (!HasComponent<ClubDescription>(data.OptionalClub))
					throw new InvalidOperationException(GameWorld.DebugCreateErrorMessage(data.OptionalClub.Handle, "Not a Club entity"));
				AddComponent(entity, new Relative<ClubDescription>(data.OptionalClub));
			}
		}
	}
}