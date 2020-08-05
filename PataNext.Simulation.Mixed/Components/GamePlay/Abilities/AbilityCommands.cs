using GameHost.Native;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.GamePlay.Abilities
{
	public struct AbilityCommands : IComponentData
	{
		/// <summary>
		///     The command used for chaining.
		/// </summary>
		public GameResource<RhythmCommandResource> Chaining;

		/// <summary>
		///     Combo command list, excluding the chaining command.
		/// </summary>
		public FixedBuffer32<GameResource<RhythmCommandResource>> Combos; //< 32 bytes should suffice, it would be 4 combo commands...

		/// <summary>
		///     Allowed commands for chaining in hero mode.
		/// </summary>
		public FixedBuffer64<GameResource<RhythmCommandResource>> HeroModeAllowedCommands; //< 64 bytes should suffice, it would be up to 8 commands...
	}
}