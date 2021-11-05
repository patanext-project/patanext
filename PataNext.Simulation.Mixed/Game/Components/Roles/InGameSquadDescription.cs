using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Roles.Interfaces;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct InGameSquadDescription : IEntityDescription
	{

	}

	public struct SquadEntityContainer : IComponentBuffer
	{
		public GameEntity Value;

		public SquadEntityContainer(GameEntity entity) => Value = entity;
	}
}