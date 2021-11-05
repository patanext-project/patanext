using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GameModes.City
{
	public struct CityScenes : IComponentBuffer
	{
		public GameEntity Entity;

		public CityScenes(GameEntity entity)
		{
			Entity = entity;
		}
	}
	
	public struct CityLocationTag : IComponentData {}

	public struct PlayerCurrentCityLocation : IComponentData
	{
		public GameEntity Entity;

		public PlayerCurrentCityLocation(GameEntity entity)
		{
			Entity = entity;
		}
	}
}