using System;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Abilities
{
	[Flags]
	public enum EAbilityPhase
	{
		None             = 0,
		WillBeActive     = 1 << 0,
		Active           = 1 << 1,
		Chaining         = 1 << 2,
		ActiveOrChaining = Active | Chaining,

		/// <summary>
		///     This state is used when the Hero mode is getting activated since they do possess a delay of a beat...
		/// </summary>
		HeroActivation = 1 << 3
	}

	public struct AbilityState : IComponentData
	{
		public EAbilityPhase Phase;

		public int Combo;
		public int ImperfectCountWhileActive;

		public int UpdateVersion;
		public int ActivationVersion;

		public bool IsActive           => (Phase & EAbilityPhase.Active) != 0;
		public bool IsActiveOrChaining => (Phase & EAbilityPhase.ActiveOrChaining) != 0;
		public bool IsChaining         => (Phase & EAbilityPhase.Chaining) != 0;

		public class Register : RegisterGameHostComponentData<AbilityState>
		{
		}
	}
}