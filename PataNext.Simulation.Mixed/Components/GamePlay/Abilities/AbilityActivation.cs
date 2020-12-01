using System;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Abilities
{
	[Flags]
	public enum EAbilityActivationType
	{
		/// <summary>
		/// This is a normal ability
		/// </summary>
		NoConstraints = 0b000_0000,

		/// <summary>
		/// This is a Hero Mode ability, which require fever and perfect conditions
		/// </summary>
		HeroMode = 0b000_0010,

		/// <summary>
		/// This is an ability that must be done on a mount
		/// </summary>
		Mount = 0b000_0100,

		/// <summary>
		/// This is an ability that must be done unmounted
		/// </summary>
		Unmounted = 0b000_1000,

		/// <summary>
		/// Require a custom condition (special func component)
		/// </summary>
		Custom = 0b100_0000
	}

	public struct AbilityActivation : IComponentData
	{
		public EAbilityActivationType Type;
		public int                    HeroModeMaxCombo;
		public int                    HeroModeImperfectLimitBeforeDeactivation;

		public int DefaultCooldownOnActivation;

		public AbilitySelection Selection;

		public class Register : RegisterGameHostComponentData<AbilityActivation>
		{
		}
	}
}