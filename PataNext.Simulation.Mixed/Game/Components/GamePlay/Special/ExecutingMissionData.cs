using DefaultEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Special
{
	public struct ExecutingMissionData : IComponentData
	{
		public Entity Target;

		public ExecutingMissionData(Entity target)
		{
			Target = target;
		}
	}
}