using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Game.RhythmEngine;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct RhythmEngineCommandProgressBufferSnapshot
	{
		public class Serializer : ArchetypeOnlySerializerBase<RhythmEngineCommandProgressBuffer>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}

	public struct RhythmEnginePredictedCommandBufferSnapshot
	{
		public class Serializer : ArchetypeOnlySerializerBase<RhythmEnginePredictedCommandBuffer>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}

	public struct RhythmCommandActionBufferSnapshot : IReadWriteSnapshotData<RhythmCommandActionBufferSnapshot>, ISnapshotSyncWithComponent<RhythmCommandActionBuffer>
	{
		public class Serializer : DeltaBufferSerializerBase<RhythmCommandActionBufferSnapshot, RhythmCommandActionBuffer>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public uint Tick { get; set; }

		public int Target;
		public int SliderLength;
		public int Offset;
		public int Key;

		public void Serialize(in BitBuffer buffer, in RhythmCommandActionBufferSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(Target, baseline.Target)
			      .AddIntDelta(SliderLength, baseline.SliderLength)
			      .AddIntDelta(Offset, baseline.Offset)
			      .AddIntDelta(Key, baseline.Key);
		}

		public void Deserialize(in BitBuffer buffer, in RhythmCommandActionBufferSnapshot baseline, in EmptySnapshotSetup setup)
		{
			Target       = buffer.ReadIntDelta(baseline.Target);
			SliderLength = buffer.ReadIntDelta(baseline.SliderLength);
			Offset       = buffer.ReadIntDelta(baseline.Offset);
			Key          = buffer.ReadIntDelta(baseline.Key);
		}

		public void FromComponent(in RhythmCommandActionBuffer component, in EmptySnapshotSetup setup)
		{
			Target       = component.Value.Beat.Target;
			SliderLength = component.Value.Beat.sliderLength;
			Offset       = (int) component.Value.Beat.offset * 1000;
			Key          = component.Value.Key;
		}

		public void ToComponent(ref RhythmCommandActionBuffer component, in EmptySnapshotSetup setup)
		{
			component = new RhythmCommandActionBuffer(new RhythmCommandAction(new Beat
			{
				Target       = Target,
				SliderLength = SliderLength,
				Offset       = Offset * 0.001f
			}, Key));
		}
	}
}