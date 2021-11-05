using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Team
{
	public readonly struct ContributeToTeamMovableArea : IComponentData
	{
		public readonly float Center;
		public readonly float Size;

		public ContributeToTeamMovableArea(float center, float size)
		{
			Center = center;
			Size   = size;
		}
	}
}