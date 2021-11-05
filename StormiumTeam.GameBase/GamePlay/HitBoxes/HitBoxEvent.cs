using System.Numerics;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay.HitBoxes
{
	public struct HitBoxEvent : IComponentData
	{
		public GameEntity HitBox;
		public GameEntity Instigator;
		public GameEntity Victim;

		public Vector3 ContactPosition;
		public Vector3 ContactNormal;
	}
}