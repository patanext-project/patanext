using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.Units
{
	public struct UnitCurrentRole : IComponentData
	{
		public readonly GameResource<UnitRoleResource> Resource;

		public UnitCurrentRole(GameResource<UnitRoleResource> id)
		{
			Resource = id;
		}
	}
}