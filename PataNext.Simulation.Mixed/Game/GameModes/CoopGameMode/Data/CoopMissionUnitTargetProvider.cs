using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.GameModes.DataCoopMission
{
	public class CoopMissionUnitTargetProvider : BaseProvider<CoopMissionUnitTargetProvider.Create>
	{
		public struct Create
		{
			public GameEntity    Player;
			public GameEntity    Team;
			public UnitDirection Direction;
		}

		public CoopMissionUnitTargetProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				AsComponentType<UnitTargetDescription>(),
				AsComponentType<UnitEnemySeekingState>(),
				AsComponentType<UnitDirection>(),
				AsComponentType<Position>(),

				AsComponentType<Relative<PlayerDescription>>(),
				AsComponentType<Relative<TeamDescription>>(),
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			if (data.Direction.Invalid) throw new InvalidOperationException("Invalid Direction");
			if (!GameWorld.Exists(data.Player)) throw new InvalidOperationException($"Invalid Player Entity {data.Player}");
			if (!GameWorld.Exists(data.Team)) throw new InvalidOperationException($"Invalid Team Entity {data.Team}");

			GetComponentData<UnitDirection>(entity)               = data.Direction;
			GetComponentData<Relative<PlayerDescription>>(entity) = new Relative<PlayerDescription>(data.Player);
			GetComponentData<Relative<TeamDescription>>(entity)   = new Relative<TeamDescription>(data.Team);
		}
	}
}