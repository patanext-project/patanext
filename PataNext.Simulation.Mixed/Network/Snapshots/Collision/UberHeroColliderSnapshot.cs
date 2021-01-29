using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Special;

namespace PataNext.Module.Simulation.Network.Snapshots.Collision
{
	public struct UberHeroColliderSnapshot : IReadWriteSnapshotData<UberHeroColliderSnapshot>, ISnapshotSyncWithComponent<UberHeroCollider>
	{
		public class Serializer : DeltaSnapshotSerializerBase<UberHeroColliderSnapshot, UberHeroCollider>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
		}

		public uint Tick { get; set; }

		private static BoundedRange range = new BoundedRange(-100, +100, 0.001f);

		public uint Scale;

		public void Serialize(in BitBuffer buffer, in UberHeroColliderSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddUIntD4Delta(Scale, baseline.Scale);
		}

		public void Deserialize(in BitBuffer buffer, in UberHeroColliderSnapshot baseline, in EmptySnapshotSetup setup)
		{
			Scale = buffer.ReadUIntD4Delta(baseline.Scale);
		}

		public void FromComponent(in UberHeroCollider component, in EmptySnapshotSetup setup)
		{
			Scale = range.Quantize(component.Scale);
		}

		public void ToComponent(ref UberHeroCollider component, in EmptySnapshotSetup setup)
		{
			component.Scale = range.Dequantize(Scale);
		}
	}
}