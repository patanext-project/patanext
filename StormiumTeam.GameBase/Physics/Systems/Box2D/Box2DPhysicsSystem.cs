using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using BepuPhysics.Collidables;
using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Common;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;
using Box2DWorld = Box2D.NetStandard.Dynamics.World.World;

namespace StormiumTeam.GameBase.Physics.Systems
{
	public class Box2DPhysicsSystem : GameAppSystem, IPhysicsSystem
	{
		public Box2DPhysicsSystem([NotNull] WorldCollection collection) : base(collection)
		{
		}

		private Box2DPhysicsColliderComponentBoard componentBoard;
		private ComponentType                      componentType;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			componentBoard = new Box2DPhysicsColliderComponentBoard(this, sizeof(byte), 0);
			componentType  = GameWorld.RegisterComponent("Box2D.Shape", componentBoard, typeof(Shape));
		}

		public ArraySegment<Shape> GetShape(GameEntityHandle entity)
		{
			var metadata = GameWorld.GetComponentMetadata(entity, componentType);
			if (metadata.Null)
				return ArraySegment<Shape>.Empty;

			return componentBoard.GetShape(metadata.Id);
		}

		public void AssignCollider(GameEntityHandle entity, Span<Shape> shapes)
		{
			GameWorld.AssureComponents(entity, stackalloc [] { componentType, AsComponentType<PhysicsCollider>() });
			
			var     metadata = GameWorld.GetComponentMetadata(entity, componentType);
			ref var segment  = ref componentBoard.GetShape(metadata.Id);
			if (segment.Count < shapes.Length)
			{
				segment = new ArraySegment<Shape>(new Shape[shapes.Length]);
			}

			shapes.CopyTo(segment.AsSpan());
			segment = segment.Slice(0, shapes.Length);
		}

		public void AssignCollider(GameEntityHandle entity, Shape shape)
		{
			GameWorld.AssureComponents(entity, stackalloc [] { componentType, AsComponentType<PhysicsCollider>() });
			
			var     metadata = GameWorld.GetComponentMetadata(entity, componentType);
			ref var segment  = ref componentBoard.GetShape(metadata.Id);
			if (segment.Count == 0)
			{
				segment = new ArraySegment<Shape>(new Shape[1]);
			}

			segment[0] = shape;
			segment    = segment.Slice(0, 1);
		}

		public void AssignCollider(GameEntityHandle entity, Entity settings)
		{
			if (settings.TryGet(out Shape shape))
			{
				AssignCollider(entity, shape);
				return;
			}

			// bepu conversion
			if (settings.TryGet(out Box box))
			{
				AssignCollider(entity, new PolygonShape(box.HalfWidth, box.HalfHeight));
				return;
			}

			throw new NullReferenceException("Invalid Settings entity");
		}

		public bool Overlap(GameEntityHandle left, GameEntityHandle right)
		{
			var shapesA = GetShape(left);
			var shapesB = GetShape(right);

			if (shapesA.Count == 0 || shapesB.Count == 0)
				return false;

			foreach (var shapeA in shapesA)
			{
				var proxyA = new DistanceProxy();
				proxyA.Set(shapeA, 0);

				var transformA = new Box2D.NetStandard.Common.Transform();
				if (TryGetComponentData(left, out Position posA))
					transformA.Set(posA.Value.XY(), 0);

				foreach (var shapeB in shapesB)
				{
					var proxyB = new DistanceProxy();
					proxyB.Set(shapeB, 1);

					var transformB = new Box2D.NetStandard.Common.Transform();
					if (TryGetComponentData(right, out Position posB))
						transformB.Set(posB.Value.XY(), 0);

					var cache = new SimplexCache();
					Contact.Distance(out var output, cache, new DistanceInput
					{
						proxyA     = proxyA, proxyB         = proxyB,
						transformA = transformA, transformB = transformB,
						useRadii   = true
					});

					if (output.distance <= 0)
						return true;
				}
			}

			return false;
		}

		private WorldManifold cachedWorldManifold = new();

		public bool Distance(GameEntityHandle against, GameEntityHandle origin, float maxDistance, EntityOverrides? overrideA, EntityOverrides? overrideB, out DistanceResult distanceResult)
		{			
			var shapesA = GetShape(against);
			var shapesB = GetShape(origin);

			if (shapesA.Count == 0 || shapesB.Count == 0)
			{
				distanceResult = default;
				return false;
			}

			distanceResult = default;
			foreach (var shapeA in shapesA)
			{
				var fixtureA = new Fixture();
				fixtureA.Create(shapeA);

				var transformA = new Box2D.NetStandard.Common.Transform();
				if (overrideA != null)
					transformA.Set(overrideA.Value.Position.XY(), 0);
				else if (TryGetComponentData(against, out Position posA))
					transformA.Set(posA.Value.XY(), 0);

				foreach (var shapeB in shapesB)
				{
					var fixtureB = new Fixture();
					fixtureB.Create(shapeB);


					var transformB = new Box2D.NetStandard.Common.Transform();
					if (overrideB != null)
						transformB.Set(overrideB.Value.Position.XY(), 0);
					else if (TryGetComponentData(origin, out Position posB))
						transformB.Set(posB.Value.XY(), 0);

					var contact = Contact.Create(fixtureA, 0, fixtureB, 1);
					Debug.Assert(contact != null, "contact != null");

					contact.Evaluate(out var manifold, transformA, transformB);

					cachedWorldManifold.normal = default;
					cachedWorldManifold.points.AsSpan().Clear();
					cachedWorldManifold.separations.AsSpan().Clear();
					var worldManifold = cachedWorldManifold;
					worldManifold.Initialize(manifold, transformA, shapeA.m_radius, transformB, shapeB.m_radius);

					if (manifold.pointCount == 0)
						continue;

					distanceResult.Distance = new ArraySegment<float>(worldManifold.separations).Average();
					distanceResult.Normal   = new Vector3(worldManifold.normal, 0);
					distanceResult.Position = new Vector3(worldManifold.points[0], 0);

					for (var i = 1; i < manifold.pointCount; i++)
						distanceResult.Position = Vector3.Lerp(distanceResult.Position, new Vector3(worldManifold.points[i], 0), 0.5f);

					if (distanceResult.Normal != Vector3.Zero && overrideB is {StopAtFirstResult: true})
						return true;
				}

				if (distanceResult.Normal != Vector3.Zero && overrideA is {StopAtFirstResult: true})
					return true;
			}

			return distanceResult.Normal != Vector3.Zero;
		}
	}
}