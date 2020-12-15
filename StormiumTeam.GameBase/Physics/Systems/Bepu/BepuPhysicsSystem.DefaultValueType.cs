using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace StormiumTeam.GameBase.Physics.Systems
{
	public struct HitResult
	{
		public float   time0,       time1;
		public Vector3 position, normal;
	}
	
	public struct AlwaysTrueSweepFilter : ISweepFilter
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowTest(int childA, int childB)
		{
			return true;
		}
	}

	public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
	{
		public void PrepareForIntegration(float dt)
		{
		}

		public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
		{
		}

		public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.ConserveMomentum;
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

		public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
		{
			return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
		}

		public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
		{
			pairMaterial.FrictionCoefficient     = 1f;
			pairMaterial.MaximumRecoveryVelocity = 2f;
			pairMaterial.SpringSettings          = ContactSpringiness;
			return true;
		}

		public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
		{
			return true;
		}

		public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
		{
			return true;
		}

		public void Dispose()
		{
		}
	}
}