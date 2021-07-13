using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GameModes
{
	public struct BasicTestGameMode : IComponentData
	{

		public class Register : RegisterGameHostComponentData<BasicTestGameMode>
		{}
	}
}