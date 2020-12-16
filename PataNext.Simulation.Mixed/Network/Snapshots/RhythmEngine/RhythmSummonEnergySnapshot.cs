using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct RhythmSummonEnergySnapshot : IReadWriteSnapshotData<RhythmSummonEnergySnapshot>, ISnapshotSyncWithComponent<RhythmSummonEnergy>
	{
		public class Serializer : DeltaSnapshotSerializerBase<RhythmSummonEnergySnapshot, RhythmSummonEnergy>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}

			protected override IAuthorityArchetype? GetAuthorityArchetype()
			{
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public uint Tick { get; set; }

		public int Value;

		public void Serialize(in BitBuffer buffer, in RhythmSummonEnergySnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(Value, baseline.Value);
		}

		public void Deserialize(in BitBuffer buffer, in RhythmSummonEnergySnapshot baseline, in EmptySnapshotSetup setup)
		{
			Value = buffer.ReadIntDelta(baseline.Value);
		}

		public void FromComponent(in RhythmSummonEnergy component, in EmptySnapshotSetup setup)
		{
			Value = component.Value;
		}

		public void ToComponent(ref RhythmSummonEnergy component, in EmptySnapshotSetup setup)
		{
			component.Value = Value;
		}
	}

	public struct RhythmSummonEnergyMaxSnapshot : IReadWriteSnapshotData<RhythmSummonEnergyMaxSnapshot>, ISnapshotSyncWithComponent<RhythmSummonEnergyMax>
	{
		public class Serializer : DeltaSnapshotSerializerBase<RhythmSummonEnergyMaxSnapshot, RhythmSummonEnergyMax>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
		}

		public uint Tick { get; set; }

		public int MaxValue;

		public void Serialize(in BitBuffer buffer, in RhythmSummonEnergyMaxSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(MaxValue, baseline.MaxValue);
		}

		public void Deserialize(in BitBuffer buffer, in RhythmSummonEnergyMaxSnapshot baseline, in EmptySnapshotSetup setup)
		{
			MaxValue = buffer.ReadIntDelta(baseline.MaxValue);
		}

		public void FromComponent(in RhythmSummonEnergyMax component, in EmptySnapshotSetup setup)
		{
			MaxValue = component.MaxValue;
		}

		public void ToComponent(ref RhythmSummonEnergyMax component, in EmptySnapshotSetup setup)
		{
			component.MaxValue = MaxValue;
		}
	}
}