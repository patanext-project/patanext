using GameHost.Native.Char;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Units
{
	public struct UnitDefinedAbilities : IComponentBuffer
	{
		public CharBuffer128    Id;
		public AbilitySelection Selection;

		public UnitDefinedAbilities(CharBuffer128 id, AbilitySelection selection)
		{
			Id        = id;
			Selection = selection;
		}
	}
}