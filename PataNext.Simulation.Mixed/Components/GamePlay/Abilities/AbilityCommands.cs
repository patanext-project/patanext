using GameHost.Native;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
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

		// TODO: See if it is really useful for the client to know the commands of a runtime ability
		// (I guess it would if the client would simulate the sounds, but in that case we could just make a backend)
		public class Register : RegisterGameHostComponentData<AbilityCommands>
		{
		}
	}
}