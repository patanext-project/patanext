using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Units;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct UnitTargetOffsetSnapshot : IReadWriteSnapshotData<UnitTargetOffsetSnapshot>, ISnapshotSyncWithComponent<UnitTargetOffset>
	{
		public class Serializer : DeltaComponentSerializerBase<UnitTargetOffsetSnapshot, UnitTargetOffset>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
		
		public uint Tick { get; set; }

		private static readonly BoundedRange Range = new BoundedRange(-100, 100, 0.1f);
		
		public uint Attack, Idle;
		
		public void Serialize(in     BitBuffer        buffer,    in UnitTargetOffsetSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddUIntD4Delta(Attack, baseline.Attack);
			buffer.AddUIntD4Delta(Idle, baseline.Idle);
		}

		public void Deserialize(in   BitBuffer        buffer,    in UnitTargetOffsetSnapshot baseline, in EmptySnapshotSetup setup)
		{
			Attack = buffer.ReadUIntD4Delta(baseline.Attack);
			Idle   = buffer.ReadUIntD4Delta(baseline.Idle);
		}

		public void FromComponent(in UnitTargetOffset component, in EmptySnapshotSetup       setup)
		{
			Attack = Range.Quantize(component.Attack);
			Idle   = Range.Quantize(component.Idle);
		}

		public void ToComponent(ref  UnitTargetOffset component, in EmptySnapshotSetup       setup)
		{
			component.Attack = Range.Dequantize(Attack);
			component.Idle   = Range.Dequantize(Idle);
		}
	}
}