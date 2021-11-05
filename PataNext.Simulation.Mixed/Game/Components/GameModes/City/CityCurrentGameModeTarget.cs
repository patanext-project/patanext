using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GameModes.City
{
	public struct CityCurrentGameModeTarget : IComponentData
	{
		public GameEntity Entity;

		public CityCurrentGameModeTarget(GameEntity entity)
		{
			Entity = entity;
		}
	}
}