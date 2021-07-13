using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components.Units;

namespace PataNext.Module.Simulation.Components.GamePlay.Structures
{
	public struct EndFlagStructure : IComponentData
	{
		public struct AllowedEntities : IComponentBuffer
		{
			public GameEntity Entity;

			public AllowedEntities(GameEntity entity) => Entity = entity;
		}

		public struct AllowedTeams : IComponentBuffer
		{
			public GameEntity Entity;

			public AllowedTeams(GameEntity entity) => Entity = entity;
		}
	}

	public struct EndFlagHasBeenPassed : IComponentData
	{
	}
}