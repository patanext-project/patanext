using System;
using BepuPhysics.Collidables;
using Box2D.NetStandard.Collision.Shapes;
using GameHost.Simulation.TabEcs;

namespace StormiumTeam.GameBase.Physics.Systems
{
	public class Box2DPhysicsColliderComponentBoard : SingleComponentBoard
	{
		public readonly Box2DPhysicsSystem physicsSystem;

		// pain to put the ? here in my IDE
		private (ArraySegment<Shape>[] shape, byte _) column;
		
		public Box2DPhysicsColliderComponentBoard(Box2DPhysicsSystem system, int size, int capacity) : base(size, capacity)
		{
			physicsSystem = system;

			column.shape = Array.Empty<ArraySegment<Shape>>();
		}

		public ref ArraySegment<Shape> GetShape(uint row)
		{
			return ref column.shape[row];
		}

		public override uint CreateRow()
		{
			var id = base.CreateRow();
			OnResize();
			return id;
		}

		protected override void OnResize()
		{
			base.OnResize();

			var prevLength = column.shape.Length;
			Array.Resize(ref column.shape, (int) board.MaxId + 1);
			for (var i = prevLength; i < column.shape.Length; i++)
				column.shape[i] = ArraySegment<Shape>.Empty;
		}

		public override bool DeleteRow(uint row)
		{
			if (!base.DeleteRow(row))
				return false;

			column.shape[row] = null;
			return true;
		}
	}
}