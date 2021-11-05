using System.Numerics;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay.HitBoxes
{
	public readonly struct HitBox : IComponentData
	{
		/// <summary>
		/// The source of this hitbox (character, turret, ...)
		/// </summary>
		public readonly GameEntity Instigator;

		public readonly int  MaxHits;
		public readonly bool VelocityUseDelta;

		public HitBox(GameEntity instigator, int maxHits, bool velocityUseDelta = true)
		{
			Instigator       = instigator;
			MaxHits          = maxHits;
			VelocityUseDelta = velocityUseDelta;
		}
	}

	public readonly struct HitBoxHistory : IComponentBuffer
	{
		public readonly GameEntity Victim;
		public readonly Vector3    Position;
		public readonly Vector3    Normal;

		public HitBoxHistory(GameEntity victim, Vector3 position, Vector3 normal)
		{
			Victim   = victim;
			Position = position;
			Normal   = normal;
		}
	}

	public readonly struct HitBoxAgainstEnemies : IComponentData
	{
		public readonly GameEntity EnemyBufferSource;

		public HitBoxAgainstEnemies(GameEntity enemyBufferSource)
		{
			EnemyBufferSource = enemyBufferSource;
		}
	}
}