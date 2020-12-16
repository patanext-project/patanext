using System;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed.Defaults;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;

namespace PataNext.CoreAbilities.Server.PartyCommand
{
	public class DefaultParty : AbilityScriptModule<DefaultPartyAbilityProvider>
	{
		private IManagedWorldTime          worldTime;
		private ExecuteActiveAbilitySystem executeActiveAbility;

		public DefaultParty(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref executeActiveAbility);
		}

		protected override void OnSetup(GameEntity self)
		{
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
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
						ability.TickProgression = default;
						executeActiveAbility.Post.Schedule(addSummonEnergy, (engineSet.Engine, energy * ability.EnergyPerTick), default);
					}
				}

				if (isActivationFrame)
					executeActiveAbility.Post.Schedule(addSummonEnergy, (engineSet.Engine, ability.EnergyOnActivation), default);
			}
			else
				ability.TickProgression = default;
		}

		private void addSummonEnergy((GameEntityHandle engine, int add) args)
		{
			GetComponentData<RhythmSummonEnergy>(args.engine).Value += args.add;
		}
	}
}