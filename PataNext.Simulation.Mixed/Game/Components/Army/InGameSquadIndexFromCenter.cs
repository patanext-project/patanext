using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Army
{
	public struct InGameSquadIndexFromCenter : IComponentData
	{
		public int   Value;

		public InGameSquadIndexFromCenter(int value)
		{
			Value = value;
		}
	}
}