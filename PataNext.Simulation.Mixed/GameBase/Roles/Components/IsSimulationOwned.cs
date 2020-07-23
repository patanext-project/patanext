using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace GameBase.Roles.Components
{
	/// <summary>
	/// Is this entity simulated by us?
	/// </summary>
	public struct IsSimulationOwned : IComponentData
	{
		public class Register : RegisterGameHostComponentData<IsSimulationOwned>
		{}
	}
}