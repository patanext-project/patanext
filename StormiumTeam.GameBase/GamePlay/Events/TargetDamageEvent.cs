using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay.Events
{
	public struct TargetDamageEvent : IComponentData
	{
		public GameEntity Instigator;
		public GameEntity Victim;

		public double Damage;

		public TargetDamageEvent(GameEntity instigator, GameEntity victim, double damage)
		{
			Instigator = instigator;
			Victim     = victim;

			Damage = damage;
		}
	}
}