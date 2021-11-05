using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Hideout
{
	/// <summary>
	/// Represent a squad with a leader. All entities that are not the leader are soldiers
	/// </summary>
	public struct HideoutLeaderSquad : IComponentData
	{
		public GameEntity Leader;
	}
}