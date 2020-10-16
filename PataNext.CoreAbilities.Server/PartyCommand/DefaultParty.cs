using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed.Defaults;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;

namespace PataNext.CoreAbilities.Server.PartyCommand
{
	public class DefaultParty : AbilityScriptModule<DefaultPartyAbilityProvider>
	{
		private IManagedWorldTime worldTime;

		public DefaultParty(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		protected override void OnSetup(GameEntity self)
		{
			Console.WriteLine("Setup Party!");
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, AbilityState state)
		{
			ref var ability = ref GetComponentData<DefaultPartyAbility>(self);
			if (!state.IsActive)
			{
				ability.TickProgression = default;
				ability.WasActive       = false;
				return;
			}

			var isActivationFrame = false;
			if (!ability.WasActive)
				isActivationFrame = ability.WasActive = true;
			ref readonly var engineSet = ref GetComponentData<AbilityEngineSet>(self);
			if (engineSet.ComboSettings.CanEnterFever(engineSet.ComboState))
			{
				ability.TickProgression += worldTime.Delta;
				if (ability.TickProgression > TimeSpan.Zero)
				{
					var energy = (int) (ability.TickProgression / ability.TickPerSecond);
					if (energy > 0)
					{
						ability.TickProgression                                      =  default;
						GetComponentData<RhythmSummonEnergy>(engineSet.Engine).Value += energy * ability.EnergyPerTick;
					}
				}

				if (isActivationFrame)
					GetComponentData<RhythmSummonEnergy>(engineSet.Engine).Value += ability.EnergyOnActivation;
			}
			else
				ability.TickProgression = default;
		}
	}
}