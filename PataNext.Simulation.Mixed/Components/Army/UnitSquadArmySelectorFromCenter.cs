using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Army
{
	/// <summary>
	/// Which army will this unit select?
	/// </summary>
	public struct UnitSquadArmySelectorFromCenter : IComponentData
	{
		public int Value;

		public UnitSquadArmySelectorFromCenter(int value) => Value = value;
	}

	public struct UnitIndexInSquad : IComponentData
	{
		public int Value;

		public UnitIndexInSquad(int index) => Value = index;
	}
}