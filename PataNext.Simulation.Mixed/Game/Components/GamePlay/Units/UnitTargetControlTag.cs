using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Units
{
	public struct UnitTargetControlTag : IComponentData
	{
		public class Register : RegisterGameHostComponentData<UnitTargetControlTag>
		{
		}
	}
}