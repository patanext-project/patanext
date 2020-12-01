using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay.Health
{
	/// <summary>
	/// Represent the health of a livable.
	/// </summary>
	public struct LivableHealth : IComponentData
	{
		public int Value, Max;
	}

	/// <summary>
	/// A tag that represent a dead livable
	/// </summary>
	public struct LivableIsDead : IComponentData
	{
		
	}
}