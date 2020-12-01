using System;

namespace PataNext.CoreAbilities.Mixed.CYumi
{
	public struct YumiyachaPierceAttackAbility : ISimpleAttackAbility
	{
		public TimeSpan AttackStart       { get; set; }
		public bool     DidAttack         { get; set; }
		public TimeSpan Cooldown          { get; set; }
		public TimeSpan DelayBeforeAttack { get; set; }
		public TimeSpan PauseAfterAttack  { get; set; }
	}
}