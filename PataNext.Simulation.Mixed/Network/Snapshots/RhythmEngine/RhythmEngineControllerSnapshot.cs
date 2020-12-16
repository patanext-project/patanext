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
	public struct RhythmEngineControllerSnapshot : IReadWriteSnapshotData<RhythmEngineControllerSnapshot>, ISnapshotSyncWithComponent<RhythmEngineController>
	{
		public class Serializer : DeltaSnapshotSerializerBase<RhythmEngineControllerSnapshot, RhythmEngineController>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
		}

		public uint Tick { get; set; }

		public uint State;
		public long StartTime;

		public void Serialize(in BitBuffer buffer, in RhythmEngineControllerSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddUIntD4Delta(State, baseline.State)
			      .AddLongDelta(StartTime, baseline.StartTime);
		}

		public void Deserialize(in BitBuffer buffer, in RhythmEngineControllerSnapshot baseline, in EmptySnapshotSetup setup)
		{
			State     = buffer.ReadUIntD4Delta(baseline.State);
			StartTime = buffer.ReadLongDelta(baseline.StartTime);
		}

		public void FromComponent(in RhythmEngineController component, in EmptySnapshotSetup setup)
		{
			State     = (uint) component.State;
			StartTime = component.StartTime.Ticks;
		}

		public void ToComponent(ref RhythmEngineController component, in EmptySnapshotSetup setup)
		{
			component.State     = (RhythmEngineState) State;
			component.StartTime = new TimeSpan(StartTime);
		}
	}
}