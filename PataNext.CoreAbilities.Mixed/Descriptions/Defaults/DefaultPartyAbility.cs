using System;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
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

		public struct Snapshot : IReadWriteSnapshotData<Snapshot>, ISnapshotSyncWithComponent<DefaultPartyAbility>
		{
			public uint Tick { get; set; }

			public long TickPerSecond;
			public int  EnergyPerTick, EnergyOnActivation;
			public void Serialize(in     BitBuffer           buffer,    in Snapshot           baseline, in EmptySnapshotSetup setup)
			{
				buffer.AddLongDelta(TickPerSecond, baseline.TickPerSecond)
				      .AddIntDelta(EnergyPerTick, baseline.EnergyPerTick)
				      .AddIntDelta(EnergyOnActivation, baseline.EnergyOnActivation);
			}

			public void Deserialize(in   BitBuffer           buffer,    in Snapshot           baseline, in EmptySnapshotSetup setup)
			{
				TickPerSecond      = buffer.ReadLongDelta(baseline.TickPerSecond);
				EnergyPerTick      = buffer.ReadIntDelta(baseline.EnergyPerTick);
				EnergyOnActivation = buffer.ReadIntDelta(baseline.EnergyOnActivation);
			}

			public void FromComponent(in DefaultPartyAbility component, in EmptySnapshotSetup setup)
			{
				TickPerSecond      = component.TickPerSecond.Ticks;
				EnergyPerTick      = component.EnergyPerTick;
				EnergyOnActivation = component.EnergyOnActivation;
			}

			public void ToComponent(ref DefaultPartyAbility component, in EmptySnapshotSetup setup)
			{
				component.TickPerSecond      = TimeSpan.FromTicks(TickPerSecond);
				component.EnergyPerTick      = EnergyPerTick;
				component.EnergyOnActivation = EnergyOnActivation;
			}
		}

		public class Serializer : DeltaSnapshotSerializerBase<Snapshot, DefaultPartyAbility>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings              = false;
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}
		}
	}

	public class DefaultPartyAbilityProvider : BaseRuntimeRhythmAbilityProvider<DefaultPartyAbility>
	{
		public DefaultPartyAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => resPath.Create(new [] { "ability", "default", "party" }, ResPath.EType.MasterServer);

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