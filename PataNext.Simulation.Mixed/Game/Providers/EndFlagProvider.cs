using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Structures;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Visuals;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.Providers
{
	public struct CreateTeamEndFlag
	{
		public float         Position;
		public UnitDirection Direction;

		public GameEntity[] Teams;

		public GameResource<GameGraphicResource> GraphicResource;
	}

	public class TeamEndFlagProvider : BaseProvider<CreateTeamEndFlag>
	{
		public TeamEndFlagProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				AsComponentType<EndFlagStructure>(),
				AsComponentType<EndFlagStructure.AllowedTeams>(),
				AsComponentType<Position>(),
				AsComponentType<UnitDirection>(),
				AsComponentType<EntityVisual>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, CreateTeamEndFlag data)
		{
			if (data.Direction.Invalid) throw new InvalidOperationException("Invalid Direction");
			if (data.Teams?.Length == default) throw new InvalidOperationException("No Teams");

			GetComponentData<UnitDirection>(entity) = data.Direction;
			GetComponentData<Position>(entity)      = new Position(data.Position);
			GetBuffer<EndFlagStructure.AllowedTeams>(entity)
				.Reinterpret<GameEntity>()
				.AddRange(data.Teams);
			GetComponentData<EntityVisual>(entity) = new EntityVisual(data.GraphicResource);
		}
	}
}