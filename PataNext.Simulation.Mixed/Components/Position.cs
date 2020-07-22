using System.Numerics;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components
{
	public struct Position : IComponentData
	{
		public Vector3 Value;
	}
}