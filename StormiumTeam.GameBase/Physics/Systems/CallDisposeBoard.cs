using System;
using BepuPhysics.Collidables;
using GameHost.Simulation.TabEcs;

namespace StormiumTeam.GameBase.Physics.Systems
{
	public class CallDisposeBoard : SingleComponentBoard
	{
		public readonly PhysicsSystem physicsSystem;

		public CallDisposeBoard(PhysicsSystem system, int size, int capacity) : base(size, capacity)
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

			Console.WriteLine("disposed!");
			physicsSystem.Simulation.Shapes.RecursivelyRemoveAndDispose(currValue, physicsSystem.BufferPool);
			return true;
		}
	}
}