using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay.Health
{
	/// <summary>
	/// A <see cref="ConcreteHealthValue"/> should be put inside a Health entity. And that health entity should write back the information to this component.
	/// </summary>
	public struct ConcreteHealthValue : IComponentData
	{
		public int Value, Max;
	}
}