using System.Numerics;
using DefaultEcs;
using GameHost.Simulation.TabEcs;

namespace StormiumTeam.GameBase.Physics
{
	public interface IPhysicsSystem
	{
		void AssignCollider(GameEntityHandle entity,  Entity           settings);
		bool Overlap(GameEntityHandle        left,    GameEntityHandle right);
		bool Distance(GameEntityHandle       against, GameEntityHandle origin, float maxDistance, EntityOverrides? overrideAgainst, EntityOverrides? overrideOrigin, out DistanceResult distanceResult);
	}

	public struct EntityOverrides
	{
		public Vector3 Position;
		public Vector3 Velocity;
	}

	public struct DistanceResult
	{
		public float   Distance;
		public Vector3 Position;
		public Vector3 Normal;
	}
}