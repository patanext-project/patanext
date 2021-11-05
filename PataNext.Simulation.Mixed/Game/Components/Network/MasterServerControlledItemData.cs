using GameHost.Native.Char;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Network
{
	public struct MasterServerControlledItemData : IComponentData
	{
		public CharBuffer64 ItemGuid;

		public MasterServerControlledItemData(CharBuffer64 itemGuid)
		{
			ItemGuid = itemGuid;
		}
	}
}