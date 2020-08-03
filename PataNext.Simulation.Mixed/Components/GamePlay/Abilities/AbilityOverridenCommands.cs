using GameHost.Native;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Abilities
{
	public struct AbilityOverridenCommands : IComponentData
	{
		/// <summary>
		///     The command used for chaining.
		/// </summary>
		public GameEntity Chaining;

		/// <summary>
		///     Combo command list, excluding the chaining command.
		/// </summary>
		public FixedBuffer32<GameEntity> Combos; //< 32 bytes should suffice, it would be 4 combo commands...

		/// <summary>
		///     Allowed commands for chaining in hero mode.
		/// </summary>
		public FixedBuffer64<GameEntity> HeroModeAllowedCommands; //< 64 bytes should suffice, it would be up to 8 commands...
	}
}