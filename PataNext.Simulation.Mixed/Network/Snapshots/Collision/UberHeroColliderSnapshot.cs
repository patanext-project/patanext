using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Special;

namespace PataNext.Module.Simulation.Network.Snapshots.Collision
{
	public struct UberHeroColliderSnapshot : IReadWriteSnapshotData<UberHeroColliderSnapshot>, ISnapshotSyncWithComponent<UnitBodyCollider>
	{
		public class Serializer : DeltaSnapshotSerializerBase<UberHeroColliderSnapshot, UnitBodyCollider>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
		}

		public uint Tick { get; set; }

		private static BoundedRange sizeRange  = new BoundedRange(-100, +100, 0.001f);
		private static BoundedRange scaleRange = new BoundedRange(-10, +10, 0.001f);

		public uint Width, Height, Scale;

		public void Serialize(in BitBuffer buffer, in UberHeroColliderSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddUIntD4Delta(Width, baseline.Width);
			buffer.AddUIntD4Delta(Height, baseline.Height);
			buffer.AddUIntD4Delta(Scale, baseline.Scale);
		}

		public void Deserialize(in BitBuffer buffer, in UberHeroColliderSnapshot baseline, in EmptySnapshotSetup setup)
		{
			Width  = buffer.ReadUIntD4Delta(baseline.Width);
			Height = buffer.ReadUIntD4Delta(baseline.Height);
			Scale  = buffer.ReadUIntD4Delta(baseline.Scale);
		}

		public void FromComponent(in UnitBodyCollider component, in EmptySnapshotSetup setup)
		{
			Width  = sizeRange.Quantize(component.Width);
			Height = sizeRange.Quantize(component.Height);
			Scale  = scaleRange.Quantize(component.Scale);
		}

		public void ToComponent(ref UnitBodyCollider component, in EmptySnapshotSetup setup)
		{
			component.Width  = sizeRange.Dequantize(Width);
			component.Height = sizeRange.Dequantize(Height);
			component.Scale  = scaleRange.Dequantize(Scale);
		}
	}
}