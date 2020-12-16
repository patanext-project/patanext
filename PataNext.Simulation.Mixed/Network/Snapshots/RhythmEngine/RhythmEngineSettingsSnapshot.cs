using System;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct RhythmEngineSettingsSnapshot : IReadWriteSnapshotData<RhythmEngineSettingsSnapshot>, ISnapshotSyncWithComponent<RhythmEngineSettings>
	{
		public class Serializer : DeltaSnapshotSerializerBase<RhythmEngineSettingsSnapshot, RhythmEngineSettings>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
		}

		public uint Tick { get; set; }

		public int MaxBeat;
		public int BeatInterval;

		public void Serialize(in BitBuffer buffer, in RhythmEngineSettingsSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(MaxBeat, baseline.MaxBeat);
			buffer.AddIntDelta(BeatInterval, baseline.BeatInterval);
		}

		public void Deserialize(in BitBuffer buffer, in RhythmEngineSettingsSnapshot baseline, in EmptySnapshotSetup setup)
		{
			MaxBeat      = buffer.ReadIntDelta(baseline.MaxBeat);
			BeatInterval = buffer.ReadIntDelta(baseline.BeatInterval);
		}

		public void FromComponent(in RhythmEngineSettings component, in EmptySnapshotSetup setup)
		{
			MaxBeat      = component.MaxBeat;
			BeatInterval = (int) component.BeatInterval.Ticks;
		}

		public void ToComponent(ref RhythmEngineSettings component, in EmptySnapshotSetup setup)
		{
			component.MaxBeat      = MaxBeat;
			component.BeatInterval = new TimeSpan(BeatInterval);
		}
	}
}