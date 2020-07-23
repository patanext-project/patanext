using System.Numerics;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Units
{
	public struct UnitEnemySeekingState : IComponentData
	{
		public GameEntity Enemy;
		public float      Distance;

		public GameEntity SelfEnemy;
		public Vector3    SelfPosition;
		public float      SelfDistance;

		public class Register : RegisterGameHostComponentData<UnitEnemySeekingState>
		{
		}
	}
}