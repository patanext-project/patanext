using System;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase;

namespace PataNext.Module.Simulation.Components.GamePlay.Units
{
	public struct UnitPlayState : IComponentData
	{
		public int Attack;
		public int Defense;

		public float ReceiveDamagePercentage;

		public float MovementSpeed;
		public float MovementAttackSpeed;
		public float MovementReturnSpeed;
		public float AttackSpeed;

		public float AttackSeekRange;

		public float Weight;

		public readonly float GetAcceleration()
		{
			return Math.Clamp(MathHelper.RcpSafe(Weight), 0, 1);
		}
	}
}