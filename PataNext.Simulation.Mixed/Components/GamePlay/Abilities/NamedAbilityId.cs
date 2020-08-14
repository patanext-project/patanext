using GameHost.Native.Char;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Simulation.mixed.Components.GamePlay.Abilities
{
	public readonly struct NamedAbilityId : IComponentData
	{
		public readonly CharBuffer64 Value;

		public NamedAbilityId(CharBuffer64 value)
		{
			Value = value;
		}

		public NamedAbilityId(string value)
		{
			Value = CharBufferUtility.Create<CharBuffer64>(value);
		}
	}
}