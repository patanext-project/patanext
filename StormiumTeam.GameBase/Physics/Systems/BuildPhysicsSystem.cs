using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using GameHost.Core.Ecs;
using GameHost.Native.Fixed;
using GameHost.Worlds.Components;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Physics.Systems
{
	public class BuildPhysicsSystem : GameAppSystem
	{
		private BufferPool bufferPool;
		private Simulation simulation;

		private IManagedWorldTime worldTime;
		
		public BuildPhysicsSystem(WorldCollection collection) : base(collection)
		{
			bufferPool = new BufferPool();
			simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new PositionFirstTimestepper());
			
			DependencyResolver.Add(() => ref worldTime);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			
			simulation.Timestep((float) worldTime.Delta.TotalSeconds);
		}

		public unsafe bool Sweep(PhysicsCollider colliderA, RigidPose poseA, PhysicsCollider colliderB, RigidPose poseB)
		{
			var taskRegistry = simulation.NarrowPhase.SweepTaskRegistry;
			var task         = simulation.NarrowPhase.SweepTaskRegistry.GetTask(colliderA.Shape.Type, colliderB.Shape.Type);
			if (task == null)
				return false;
			
			simulation.Shapes[colliderA.Shape.Type].GetShapeData(colliderA.Shape.Index, out var shapePointerA, out _);
			simulation.Shapes[colliderB.Shape.Type].GetShapeData(colliderB.Shape.Index, out var shapePointerB, out _);
			
			//task.Sweep(shapePointerA, colliderA.Shape.Type,)
			
			return true;
		}
	}

	public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
	{
		public void                   PrepareForIntegration(float dt)
		{
		}

		public void                   IntegrateVelocity(int       bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
		{
		}

		public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
	}
	
	public struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
	{
		public SpringSettings ContactSpringiness;

		public void Initialize(Simulation simulation)
		{
			//Use a default if the springiness value wasn't initialized.
			if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
				ContactSpringiness = new SpringSettings(30, 1);
		}

		public bool AllowContactGeneration(int              workerIndex, CollidableReference a,    CollidableReference b)
		{
			return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
		}

		public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair      pair, ref TManifold       manifold,    out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
		{
			pairMaterial.FrictionCoefficient     = 1f;
			pairMaterial.MaximumRecoveryVelocity = 2f;
			pairMaterial.SpringSettings          = ContactSpringiness;
			return true;
		}

		public bool AllowContactGeneration(int              workerIndex, CollidablePair      pair, int                 childIndexA, int                        childIndexB)
		{
			return true;
		}

		public bool ConfigureContactManifold(int            workerIndex, CollidablePair      pair, int                 childIndexA, int                        childIndexB, ref ConvexContactManifold manifold)
		{
			return true;
		}

		public void Dispose()
		{
		}
	}
}