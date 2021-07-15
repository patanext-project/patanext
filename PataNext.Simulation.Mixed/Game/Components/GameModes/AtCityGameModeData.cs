using DefaultEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GameModes
{
	public struct AtCityGameModeData : IComponentData
	{
		public struct TargetMission : IComponentData
		{
			public Entity Target;

			public TargetMission(Entity target) => Target = target;
		}
	}
}