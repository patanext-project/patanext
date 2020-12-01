using System.Numerics;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.Transform.Components
{
	public struct Position : IComponentData
	{
		public Vector3 Value;

		public Position(float x = 0, float y = 0, float z = 0)
		{
			Value.X = x;
			Value.Y = y;
			Value.Z = z;
		}
	}
}