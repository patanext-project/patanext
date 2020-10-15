using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using Newtonsoft.Json;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.mixed.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.Simulation.Mixed.Abilities.Defaults
{
	public struct DefaultPartyAbility : IComponentData
	{
		public TimeSpan TickProgression;
		public TimeSpan TickPerSecond;
		public bool     WasActive;

		public int EnergyPerTick;
		public int EnergyOnActivation;
	}

	public class DefaultPartyAbilityProvider : BaseRhythmAbilityProvider<DefaultPartyAbility>
	{
		public DefaultPartyAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "party";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<PartyCommand>();
		}

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			if (ProvidedJson == null || !JsonConvert.DeserializeAnonymousType(ProvidedJson, new {disableEnergy = false}).disableEnergy)
			{
				GameWorld.GetComponentData<DefaultPartyAbility>(entity) = new DefaultPartyAbility
				{
					TickPerSecond      = TimeSpan.FromSeconds(0.1),
					EnergyPerTick      = 0,
					EnergyOnActivation = 150
				};
			}

			GameWorld.AddComponent(entity, new ExecutableAbility((owner, self, state) =>
			{
				Console.WriteLine($"{owner}; {self}; {state.Phase}");
			}));
		}
	}

	public class DefaultPartyAbilitySystem : BaseAbilitySystem
	{
		private IManagedWorldTime worldTime;

		public DefaultPartyAbilitySystem(WorldCollection collection) : base(collection)
		{
			World.Remove(this);
			
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery abilityQuery;

		public override void OnAbilityUpdate()
		{
			var dt = worldTime.Delta;

			var abilityAccessor   = GetAccessor<DefaultPartyAbility>();
			var stateAccessor     = GetAccessor<AbilityState>();
			var engineSetAccessor = GetAccessor<AbilityEngineSet>();
			var ownerAccessor     = GetAccessor<Owner>();
			foreach (var entity in abilityQuery ??= CreateEntityQuery(new[]
			{
				AsComponentType<DefaultPartyAbility>(),
				AsComponentType<AbilityState>(),
				AsComponentType<AbilityEngineSet>(),
				AsComponentType<Owner>()
			}))
			{
				ref var ability = ref abilityAccessor[entity];

				ref readonly var state = ref stateAccessor[entity];
				if (!state.IsActive)
				{
					ability.TickProgression = default;
					ability.WasActive       = false;
					continue;
				}

				var isActivationFrame = false;
				if (!ability.WasActive)
					isActivationFrame = ability.WasActive = true;

				ref readonly var engineSet = ref engineSetAccessor[entity];
				if (engineSet.ComboSettings.CanEnterFever(engineSet.ComboState))
				{
					ability.TickProgression += dt;
					if (ability.TickProgression > TimeSpan.Zero)
					{
						var energy = (int) (ability.TickProgression / ability.TickPerSecond);
						if (energy > 0)
						{
							ability.TickProgression = default;

							GetComponentData<RhythmSummonEnergy>(engineSet.Engine).Value += energy * ability.EnergyPerTick;
						}
					}

					if (isActivationFrame)
					{
						GetComponentData<RhythmSummonEnergy>(engineSet.Engine).Value += ability.EnergyOnActivation;
						Console.WriteLine("PARTY!");
					}
				}
				else
					ability.TickProgression = default;
			}
		}
	}
}