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
	public struct RhythmEngineLocalStateSnapshot : IReadWriteSnapshotData<RhythmEngineLocalStateSnapshot>, ISnapshotSyncWithComponent<RhythmEngineLocalState>
	{
		public class Serializer : DeltaComponentSerializerBase<RhythmEngineLocalStateSnapshot, RhythmEngineLocalState>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
		}

		public uint Tick { get; set; }

		public int RecoveryBeat;

		public void Serialize(in BitBuffer buffer, in RhythmEngineLocalStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(RecoveryBeat, baseline.RecoveryBeat);
		}

		public void Deserialize(in BitBuffer buffer, in RhythmEngineLocalStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			RecoveryBeat = buffer.ReadIntDelta(baseline.RecoveryBeat);
		}

		public void FromComponent(in RhythmEngineLocalState component, in EmptySnapshotSetup setup)
		{
			RecoveryBeat = component.RecoveryActivationBeat;
		}

		public void ToComponent(ref RhythmEngineLocalState component, in EmptySnapshotSetup setup)
		{
			component.RecoveryActivationBeat = RecoveryBeat;
		}
	}
}