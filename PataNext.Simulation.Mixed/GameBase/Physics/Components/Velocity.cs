using System.Numerics;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace GameBase.Physics.Components
{
	public struct Velocity : IComponentData
	{
		public Vector3 Value;
		
		public class Register : RegisterGameHostComponentData<Velocity>
		{}
	}
}