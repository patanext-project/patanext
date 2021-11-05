using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay.Health
{
	public enum ModifyHealthType
	{
		SetFixed,
		Add,
		SetMax,
		SetNone
	}

	public struct ModifyHealthEvent : IComponentData
	{
		public ModifyHealthType Type;
		public int              Origin;

		public int Consumed;

		public GameEntity Target;

		public ModifyHealthEvent(ModifyHealthType type, int origin, GameEntity target)
		{
			Type = type;

			Origin   = origin;
			Consumed = origin;

			Target = target;
		}
	}
}