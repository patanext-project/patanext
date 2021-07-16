using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Army
{
	public struct SquadIndexFromCenter : IComponentData
	{
		public int   Value;

		public SquadIndexFromCenter(int value)
		{
			Value = value;
		}
	}

	public struct InGameSquadOffset : IComponentData
	{
		public float Value;

		public InGameSquadOffset(float value)
		{
			Value = value;
		}
	}
}