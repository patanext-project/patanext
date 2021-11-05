using System;
using GameHost.Injection;
using GameHost.Native.Char;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Resources;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Module.Simulation.Network.Snapshots.Resources
{
	public struct EquipmentResourceSnapshot : IReadWriteSnapshotData<EquipmentResourceSnapshot>,
	                                              ISnapshotSyncWithComponent<EquipmentResource>
	{
		public class Serializer : DeltaSnapshotSerializerBase<EquipmentResourceSnapshot, EquipmentResource>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public uint Tick { get; set; }

		public CharBuffer64 Identifier;

		public void Serialize(in BitBuffer buffer, in EquipmentResourceSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (UnsafeUtility.SameData(this, baseline))
			{
				buffer.AddBool(false);
				return;
			}

			buffer.AddBool(true)
			      .AddString(Identifier.ToString());
		}

		public void Deserialize(in BitBuffer buffer, in EquipmentResourceSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			Identifier = CharBufferUtility.Create<CharBuffer64>(buffer.ReadString());
		}

		public void FromComponent(in EquipmentResource component, in EmptySnapshotSetup setup)
		{
			Identifier = component.Value;
		}

		public void ToComponent(ref EquipmentResource component, in EmptySnapshotSetup setup)
		{
			component = new EquipmentResource(Identifier);
		}
	}
}