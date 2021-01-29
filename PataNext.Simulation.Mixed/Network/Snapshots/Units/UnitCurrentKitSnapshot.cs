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
	public struct UnitCurrentKitSnapshot : IReadWriteSnapshotData<UnitCurrentKitSnapshot, GhostSetup>, ISnapshotSyncWithComponent<UnitCurrentKit, GhostSetup>
	{
		public class Serializer : DeltaSnapshotSerializerBase<UnitCurrentKitSnapshot, UnitCurrentKit, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}
		}

		public uint Tick { get; set; }

		public Ghost Ghost;

		public void Serialize(in BitBuffer buffer, in UnitCurrentKitSnapshot baseline, in GhostSetup setup)
		{
			buffer.AddGhostDelta(Ghost, baseline.Ghost);
		}

		public void Deserialize(in BitBuffer buffer, in UnitCurrentKitSnapshot baseline, in GhostSetup setup)
		{
			Ghost = buffer.ReadGhostDelta(baseline.Ghost);
		}

		public void FromComponent(in UnitCurrentKit component, in GhostSetup setup)
		{
			Ghost = setup.ToGhost(component.Resource.Entity);
		}

		public void ToComponent(ref UnitCurrentKit component, in GhostSetup setup)
		{
			component = new UnitCurrentKit(new GameResource<UnitKitResource>(setup.FromGhost(Ghost)));
		}
	}
}