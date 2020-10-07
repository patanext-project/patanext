using System.Numerics;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.Transform.Components
{
	public struct Position : IComponentData
	{
		public Vector3 Value;
	}
}