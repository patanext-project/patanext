using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
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
	
	public struct RhythmCommandActionBufferSnapshot
	{
		public class Serializer : ArchetypeOnlySerializerBase<RhythmCommandActionBuffer>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}
}