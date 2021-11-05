using System;
using BepuPhysics.Collidables;
using GameHost.Simulation.TabEcs;

namespace StormiumTeam.GameBase.Physics.Systems
{
	public class BepuPhysicsColliderComponentBoard : SingleComponentBoard
	{
		public readonly BepuPhysicsSystem physicsSystem;

		public BepuPhysicsColliderComponentBoard(BepuPhysicsSystem system, int size, int capacity) : base(size, capacity)
		{
			physicsSystem = system;
		}

		public override bool DeleteRow(uint row)
		{
			if (!base.DeleteRow(row))
				return false;

			var currValue = Read<TypedIndex>(row);
			if (!currValue.Exists)
				throw new InvalidOperationException("this should not happen");
			
			physicsSystem.Simulation.Shapes.RecursivelyRemoveAndDispose(currValue, physicsSystem.BufferPool);
			return true;
		}
	}
}