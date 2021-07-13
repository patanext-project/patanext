using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Hideout
{
	public struct HideoutSquadIndex : IComponentData
	{
		public int Value;

		public HideoutSquadIndex(int value)
		{
			Value = value;
		}
	}
}