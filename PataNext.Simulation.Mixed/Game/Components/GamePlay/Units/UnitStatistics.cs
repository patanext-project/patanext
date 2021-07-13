using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Units
{
	public struct UnitStatistics : IComponentData
	{
		public int Health;

		public int   Attack;
		public float AttackSpeed;

		public int Defense;

		public float MovementAttackSpeed;
		public float BaseWalkSpeed;
		public float FeverWalkSpeed;

		/// <summary>
		///     Weight can be used to calculate unit acceleration for moving or for knock-back power amplification.
		/// </summary>
		public float Weight;

		public float KnockbackPower;
		public float Precision;

		public float AttackMeleeRange;
		public float AttackSeekRange;

		public class Register : RegisterGameHostComponentData<UnitStatistics>
		{
		}
	}
}