using GameHost.Native.Char;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Network
{
	public struct MasterServerControlledUnitData : IComponentData
	{
		public CharBuffer64 UnitGuid;

		public MasterServerControlledUnitData(CharBuffer64 unitGuid)
		{
			UnitGuid = unitGuid;
		}
	}
	
	public struct MasterServerUnitPresetData : IComponentData
	{
		public CharBuffer64 PresetGuid;

		public MasterServerUnitPresetData(CharBuffer64 presetGuid)
		{
			PresetGuid = presetGuid;
		}
	}
}