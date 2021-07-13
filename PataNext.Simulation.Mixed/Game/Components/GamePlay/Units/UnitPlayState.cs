using System;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
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
		public float KnockbackPower;
		public float Precision;

		public readonly float GetAcceleration()
		{
			return Math.Clamp(MathUtils.RcpSafe(Weight), 0, 1);
		}
		
		public class Register : RegisterGameHostComponentData<UnitPlayState>
		{}
	}
}