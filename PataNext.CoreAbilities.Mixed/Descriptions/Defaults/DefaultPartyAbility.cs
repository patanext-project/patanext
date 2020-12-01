using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using Newtonsoft.Json;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.CoreAbilities.Mixed.Defaults
{
	public struct DefaultPartyAbility : IComponentData
	{
		public TimeSpan TickProgression;
		public TimeSpan TickPerSecond;
		public bool     WasActive;

		public int EnergyPerTick;
		public int EnergyOnActivation;
	}

	public class DefaultPartyAbilityProvider : BaseRuntimeRhythmAbilityProvider<DefaultPartyAbility>
	{
		public DefaultPartyAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "party";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<PartyCommand>();
		}

		public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
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
		}
	}
}