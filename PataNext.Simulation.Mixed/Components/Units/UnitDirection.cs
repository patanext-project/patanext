using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Units
{
	public struct UnitDirection : IComponentData
	{
		public sbyte Value;

		public readonly static UnitDirection Left  = new UnitDirection {Value = -1};
		public readonly static UnitDirection Right = new UnitDirection {Value = +1};

		public bool IsLeft  => Value == -1;
		public bool IsRight => Value == 1;

		public bool Invalid => !IsLeft && !IsRight;

		public class Register : RegisterGameHostComponentData<UnitDirection>
		{
		}
	}
}