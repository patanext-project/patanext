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
	public struct UnitArchetypeSnapshot : IReadWriteSnapshotData<UnitArchetypeSnapshot, GhostSetup>, ISnapshotSyncWithComponent<UnitArchetype, GhostSetup>
	{
		public class Serializer : DeltaComponentSerializerBase<UnitArchetypeSnapshot, UnitArchetype, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public uint Tick { get; set; }

		public Ghost Ghost;

		public void Serialize(in BitBuffer buffer, in UnitArchetypeSnapshot baseline, in GhostSetup setup)
		{
			buffer.AddGhostDelta(Ghost, baseline.Ghost);
		}

		public void Deserialize(in BitBuffer buffer, in UnitArchetypeSnapshot baseline, in GhostSetup setup)
		{
			Ghost = buffer.ReadGhostDelta(baseline.Ghost);
		}

		public void FromComponent(in UnitArchetype component, in GhostSetup setup)
		{
			Ghost = setup.ToGhost(component.Resource.Entity);
		}

		public void ToComponent(ref UnitArchetype component, in GhostSetup setup)
		{
			component = new UnitArchetype(new GameResource<UnitArchetypeResource>(setup.FromGhost(Ghost)));
		}
	}
}