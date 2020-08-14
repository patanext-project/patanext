﻿using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine;
using PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.Simulation.Mixed.Abilities.Defaults
{
	public struct DefaultPartyAbility : IComponentData
	{
		public TimeSpan TickProgression;
		public TimeSpan TickPerSecond;
		public bool  WasActive;

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

			GameWorld.GetComponentData<DefaultPartyAbility>(entity) = new DefaultPartyAbility
			{
				TickPerSecond = TimeSpan.FromSeconds(0.1),
				EnergyPerTick = 1,
				EnergyOnActivation = 30
			};
		}
	}

	public class DefaultPartyAbilitySystem : BaseAbilitySystem
	{
		private IManagedWorldTime worldTime;
		
		public DefaultPartyAbilitySystem(WorldCollection collection) : base(collection)
		{
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
			foreach (var entity in (abilityQuery ??= CreateEntityQuery(new[]
			{
				AsComponentType<DefaultPartyAbility>(),
				AsComponentType<AbilityState>(),
				AsComponentType<AbilityEngineSet>(),
				AsComponentType<Owner>()
			})).GetEntities())
			{
				ref var ability = ref abilityAccessor[entity];
				
				ref readonly var state = ref stateAccessor[entity];
				if (!state.IsActive)
				{
					ability.TickProgression = default;
					ability.WasActive       = false;
					return;
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
						GetComponentData<RhythmSummonEnergy>(engineSet.Engine).Value += ability.EnergyOnActivation;
				}
				else
					ability.TickProgression = default;
			}
		}
	}
}