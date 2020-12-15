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

		public Shape? GetShape(GameEntityHandle entity)
		{
			var metadata = GameWorld.GetComponentMetadata(entity, componentType);
			if (metadata.Null)
				return null;
			
			return componentBoard.GetShape(metadata.Id)!;
		}

		public void AssignCollider(GameEntityHandle entity, Shape shape)
		{
			var reference = GameWorld.AddComponent(entity, componentType);
			componentBoard.GetShape(reference.Id) = shape;

			AddComponent(entity, new PhysicsCollider());
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
			var shapeA = GetShape(left);
			var shapeB = GetShape(right);

			if (shapeA is null || shapeB is null)
				return false;

			var proxyA = new DistanceProxy();
			var proxyB = new DistanceProxy();
			proxyA.Set(shapeA, 0);
			proxyB.Set(shapeB, 1);

			var transformA = new Box2D.NetStandard.Common.Transform();
			var transformB = new Box2D.NetStandard.Common.Transform();
			if (TryGetComponentData(left, out Position posA))
				transformA.Set(posA.Value.XY(), 0);
			if (TryGetComponentData(right, out Position posB))
				transformB.Set(posB.Value.XY(), 0);

			var cache = new SimplexCache();
			Contact.Distance(out var output, cache, new DistanceInput
			{
				proxyA     = proxyA, proxyB         = proxyB,
				transformA = transformA, transformB = transformB,
				useRadii   = true
			});

			return output.distance <= 0;
		}

		public bool DistanceV1(GameEntityHandle against, GameEntityHandle origin, float maxDistance, EntityOverrides? overrideA, EntityOverrides? overrideB, out DistanceResult distanceResult)
		{
			var shapeA = GetShape(against);
			var shapeB = GetShape(origin);

			if (shapeA is null || shapeB is null)
			{
				distanceResult = default;
				return false;
			}

			var proxyA = new DistanceProxy();
			var proxyB = new DistanceProxy();
			proxyA.Set(shapeA, 0);
			proxyB.Set(shapeB, 1);

			var transformA = new Box2D.NetStandard.Common.Transform();
			var transformB = new Box2D.NetStandard.Common.Transform();
			if (overrideA != null)
				transformA.Set(overrideA.Value.Position.XY(), 0);
			else if (TryGetComponentData(against, out Position posA))
				transformA.Set(posA.Value.XY(), 0);

			if (overrideB != null)
				transformB.Set(overrideB.Value.Position.XY(), 0);
			else if (TryGetComponentData(origin, out Position posB))
				transformB.Set(posB.Value.XY(), 0);
			
			var cache = new SimplexCache();
			Contact.Distance(out var output, cache, new DistanceInput
			{
				proxyA     = proxyA, proxyB         = proxyB,
				transformA = transformA, transformB = transformB,
				useRadii   = true
			});

			distanceResult.Distance = output.distance;
			distanceResult.Normal   = new Vector3(output.pointB - output.pointA, 0);
			distanceResult.Position = new Vector3(output.pointA, 0);

			Console.WriteLine($"{output.distance}, {output.pointA} {output.pointB}");
			return output.distance <= 0;
		}
		
		public bool Distance(GameEntityHandle against, GameEntityHandle origin, float maxDistance, EntityOverrides? overrideA, EntityOverrides? overrideB, out DistanceResult distanceResult)
		{
			var shapeA = GetShape(against);
			var shapeB = GetShape(origin);
			
			if (shapeA is null || shapeB is null)
			{
				distanceResult = default;
				return false;
			}
			
			var fixtureA = new Fixture();
			var fixtureB = new Fixture();
			fixtureA.Create(shapeA);
			fixtureB.Create(shapeB);

			var transformA = new Box2D.NetStandard.Common.Transform();
			var transformB = new Box2D.NetStandard.Common.Transform();
			if (overrideA != null)
				transformA.Set(overrideA.Value.Position.XY(), 0);
			else if (TryGetComponentData(against, out Position posA))
				transformA.Set(posA.Value.XY(), 0);

			if (overrideB != null)
				transformB.Set(overrideB.Value.Position.XY(), 0);
			else if (TryGetComponentData(origin, out Position posB))
				transformB.Set(posB.Value.XY(), 0);

			var contact = Contact.Create(fixtureA, 0, fixtureB, 1);
			Debug.Assert(contact != null, "contact != null");
			
			contact.Evaluate(out var manifold, transformA, transformB);

			var worldManifold = new WorldManifold();
			worldManifold.Initialize(manifold, transformA, shapeA.m_radius, transformB, shapeB.m_radius);

			if (manifold.pointCount == 0)
			{
				distanceResult = default;
				return false;
			}

			distanceResult.Distance = worldManifold.separations.Average();
			distanceResult.Normal   = new Vector3(worldManifold.normal, 0);
			distanceResult.Position = new Vector3(worldManifold.points[0], 0);
			for (var i = 1; i < worldManifold.points.Length; i++)
				distanceResult.Position = Vector3.Lerp(distanceResult.Position, new Vector3(worldManifold.points[i], 0), 0.5f);

			return true;
		}
	}
}