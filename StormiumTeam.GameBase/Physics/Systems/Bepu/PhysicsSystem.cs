using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using Microsoft.Extensions.Logging;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;
using ZLogger;

namespace StormiumTeam.GameBase.Physics.Systems
{
	public class BepuPhysicsSystem : GameAppSystem, IPhysicsSystem
	{
		private ILogger logger;

		private IManagedWorldTime worldTime;

		public Simulation Simulation { get; }

		public BufferPool BufferPool { get; }

		public const float MaximumDistance = 0f;

		public BepuPhysicsSystem(WorldCollection collection) : base(collection)
		{
			BufferPool = new BufferPool();
			Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new PositionFirstTimestepper());

			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref logger);
		}

		private ComponentType                     disposeComponentType;
		private BepuPhysicsColliderComponentBoard disposeComponentBoard;

		protected override unsafe void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			disposeComponentBoard = new BepuPhysicsColliderComponentBoard(this, sizeof(TypedIndex), 0);
			disposeComponentType  = GameWorld.RegisterComponent("DisposeShapeFromTypeIndex", disposeComponentBoard);
		}

		public TypedIndex GetTypedIndex(GameEntityHandle handle)
		{
			if (!HasComponent<PhysicsCollider>(handle) || !HasComponent(handle, disposeComponentType))
				throw new InvalidOperationException($"{Safe(handle)} does not have a collider");

			return disposeComponentBoard.Read<TypedIndex>(GameWorld.GetComponentMetadata(handle, disposeComponentType).Id);
		}

		public TypedIndex SetColliderShape<TShape>(GameEntityHandle entity, TShape shape, bool disposeOnRemove = true)
			where TShape : unmanaged, IShape
		{
			if (GameWorld.HasComponent(entity, disposeComponentType))
				Simulation.Shapes.RecursivelyRemoveAndDispose(GetTypedIndex(entity), BufferPool);

			var index = Simulation.Shapes.Add(shape);
			AddComponent(entity, new PhysicsCollider());

			var disposeComponent = GameWorld.AddComponent(entity, disposeComponentType);
			disposeComponentBoard.SetValue(disposeComponent.Id, index);

			return index;
		}

		public unsafe float Distance(GameEntityHandle against,    TypedIndex    shape, RigidPose pose, BodyVelocity velocity,
		                             out HitResult    againstHit, out HitResult thisShapeHit,
		                             float            maximumT = 0)
		{
			maximumT = velocity.Linear.Length();
			
			if (!Sweep(against, shape, pose, velocity, out againstHit, maximumT))
			{
				thisShapeHit = default;
				return float.NaN;
			}

			var againstCollider = GetTypedIndex(against);
			var againstPose     = RigidPose.Identity;
			if (TryGetComponentData(against, out Position position))
				againstPose.Position = position.Value;

			Simulation.Shapes[againstCollider.Type].GetShapeData(againstCollider.Index, out var shapePointer, out _);
			Simulation.Shapes[shape.Type].GetShapeData(shape.Index, out var thisShapePtr, out _);

			var secondSweep = Sweep(
				thisShapePtr, shape.Type, pose, default,
				shapePointer, againstCollider.Type, againstPose, new BodyVelocity(againstHit.normal * 0.01f),
				out thisShapeHit,
				maximumT
			);

			if (!secondSweep)
			{
				return float.NaN;
			}

			var dist = Vector3.Distance(againstHit.position, thisShapeHit.position);
			Console.WriteLine($"{againstHit.position} {thisShapeHit.position} ({againstPose.Position} {pose.Position}) ({againstHit.time0} {againstHit.time1}, {thisShapeHit.time0} {thisShapeHit.time1})");
			return againstHit.time0 <= 0 ? -dist : dist;
		}

		public unsafe bool Sweep(GameEntityHandle against, TypedIndex shape, RigidPose pose, BodyVelocity velocity,
		                         out HitResult    hit,
		                         float            maximumT = MaximumDistance)
		{
			var againstCollider = GetTypedIndex(against);
			var againstPose     = RigidPose.Identity;
			if (TryGetComponentData(against, out Position position))
				againstPose.Position = position.Value;

			Simulation.Shapes[againstCollider.Type].GetShapeData(againstCollider.Index, out var shapePointer, out _);
			Simulation.Shapes[shape.Type].GetShapeData(shape.Index, out var thisShapePtr, out _);

			return Sweep(
				shapePointer, againstCollider.Type, againstPose, default,
				thisShapePtr, shape.Type, pose, velocity,
				out hit,
				maximumT
			);
		}

		public unsafe bool Sweep<TShape>(GameEntityHandle against, TShape collider, RigidPose pose, BodyVelocity velocity,
		                                 out HitResult    hit,
		                                 float            maximumT = MaximumDistance)
			where TShape : IShape
		{
			var againstCollider = GetTypedIndex(against);
			var againstPose     = RigidPose.Identity;
			if (TryGetComponentData(against, out Position position))
				againstPose.Position = position.Value;

			Simulation.Shapes[againstCollider.Type].GetShapeData(againstCollider.Index, out var shapePointer, out _);
			return Sweep(
				shapePointer, againstCollider.Type, againstPose, default,
				Unsafe.AsPointer(ref collider), collider.TypeId, pose, velocity,
				out hit,
				maximumT
			);
		}

		public unsafe bool Sweep(TypedIndex    colliderA, RigidPose poseA, BodyVelocity velocityA,
		                         TypedIndex    colliderB, RigidPose poseB, BodyVelocity velocityB,
		                         out HitResult hit,
		                         float         maximumT = MaximumDistance)
		{
			Simulation.Shapes[colliderA.Type].GetShapeData(colliderA.Index, out var shapePointerA, out _);
			Simulation.Shapes[colliderB.Type].GetShapeData(colliderB.Index, out var shapePointerB, out _);

			return Sweep(
				shapePointerA, colliderA.Type, poseA, velocityA,
				shapePointerB, colliderB.Type, poseB, velocityB,
				out hit,
				maximumT);
		}

		public unsafe bool Sweep(void*         a, int typeA, RigidPose poseA, BodyVelocity velocityA,
		                         void*         b, int typeB, RigidPose poseB, BodyVelocity velocityB,
		                         out HitResult hit,
		                         float         maximumT = MaximumDistance)
		{
			var task = Simulation.NarrowPhase.SweepTaskRegistry.GetTask(typeA, typeB);

			if (task == null)
			{
				logger.ZLogError("No Task Found for {0},{1}", typeA, typeB);
				hit = default;
				return false;
			}

			//Console.WriteLine($"{poseA.Position} {poseB.Position}");

			var filter = new AlwaysTrueSweepFilter();
			var intersect = task.Sweep(
				a, typeA, poseA.Orientation, velocityA,
				b, typeB, poseB.Position - poseA.Position, poseB.Orientation, velocityB,
				maximumT, 1e-2f, 1e-5f, 25, ref filter, Simulation.Shapes, Simulation.NarrowPhase.SweepTaskRegistry, BufferPool,
				out hit.time0, out hit.time1, out hit.position, out hit.normal
			);
			hit.position += poseA.Position;

			return intersect;
		}

		public void AssignCollider(GameEntityHandle entity, Entity settings)
		{
			if (settings.TryGet(out Box box))
				SetColliderShape(entity, box);
			else if (settings.TryGet(out Sphere sphere))
				SetColliderShape(entity, sphere);
			else if (settings.TryGet(out Capsule capsule))
				SetColliderShape(entity, capsule);
			else if (settings.TryGet(out Compound compound))
				SetColliderShape(entity, compound);
		}

		public bool Overlap(GameEntityHandle        left,    GameEntityHandle right)
		{
			throw new NotImplementedException();
		}

		public bool Distance(GameEntityHandle against, GameEntityHandle origin, float maxDistance, EntityOverrides? overrideAgainst, EntityOverrides? overrideOrigin, out DistanceResult distanceResult)
		{
			throw new NotImplementedException();
		}
	}
}