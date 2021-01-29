using System.Numerics;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Units
{
	public struct UnitDirection : IComponentData
	{
		public sbyte Value;

		public static readonly UnitDirection Left  = new UnitDirection {Value = -1};
		public static readonly UnitDirection Right = new UnitDirection {Value = +1};

		public bool IsLeft  => Value == -1;
		public bool IsRight => Value == 1;

		public bool    Invalid => !IsLeft && !IsRight;
		public Vector2 UnitX   => new Vector2(Value, 0);
		public Vector2 FactorX => new Vector2(Value, 1);

		public class Register : RegisterGameHostComponentData<UnitDirection>
		{
		}
	}
}