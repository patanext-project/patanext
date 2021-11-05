using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Utility.Resource;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct UnitDisplayedEquipmentSnapshot : IReadWriteSnapshotData<UnitDisplayedEquipmentSnapshot, GhostSetup>,
	                                               ISnapshotSyncWithComponent<UnitDisplayedEquipment, GhostSetup>
	{
		public class Serializer : DeltaBufferSerializerBase<UnitDisplayedEquipmentSnapshot, UnitDisplayedEquipment, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public uint Tick { get; set; }

		public Ghost Attachment, Resource;

		public void Serialize(in BitBuffer buffer, in UnitDisplayedEquipmentSnapshot baseline, in GhostSetup setup)
		{
			buffer.AddGhostDelta(Attachment, baseline.Attachment)
			      .AddGhostDelta(Resource, baseline.Resource);
		}

		public void Deserialize(in BitBuffer buffer, in UnitDisplayedEquipmentSnapshot baseline, in GhostSetup setup)
		{
			Attachment = buffer.ReadGhostDelta(baseline.Attachment);
			Resource   = buffer.ReadGhostDelta(baseline.Resource);
		}

		public void FromComponent(in UnitDisplayedEquipment component, in GhostSetup setup)
		{
			Attachment = setup.ToGhost(component.Attachment.Entity);
			Resource   = setup.ToGhost(component.Resource.Entity);
		}

		public void ToComponent(ref UnitDisplayedEquipment component, in GhostSetup setup)
		{
			component.Attachment = new GameResource<UnitAttachmentResource>(setup.FromGhost(Attachment));
			component.Resource   = new GameResource<EquipmentResource>(setup.FromGhost(Resource));
		}
	}
}