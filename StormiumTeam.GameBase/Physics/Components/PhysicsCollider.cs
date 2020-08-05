using BepuPhysics;
using BepuPhysics.Collidables;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.Physics.Components
{
	public struct PhysicsCollider : IComponentData
	{
		public TypedIndex Shape;

		public ShapeBatch GetShapeBatch(Simulation simulation)
		{
			return simulation.Shapes[Shape.Type];
		}
	}
}