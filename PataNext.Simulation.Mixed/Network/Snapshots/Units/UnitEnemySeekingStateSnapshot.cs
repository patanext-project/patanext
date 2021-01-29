using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Units;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct UnitEnemySeekingStateSnapshot :
		IReadWriteSnapshotData<UnitEnemySeekingStateSnapshot, GhostSetup>,
		ISnapshotSyncWithComponent<UnitEnemySeekingState, GhostSetup>
	{
		public class Serializer : DeltaSnapshotSerializerBase<UnitEnemySeekingStateSnapshot, UnitEnemySeekingState, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings              = false;
				CheckDifferenceSettings          = true;
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}
		}

		public uint Tick { get; set; }

		public Ghost Enemy;
		public int   Distance;

		public void Serialize(in BitBuffer buffer, in UnitEnemySeekingStateSnapshot baseline, in GhostSetup setup)
		{
			buffer.AddGhostDelta(Enemy, baseline.Enemy);
			buffer.AddIntDelta(Distance, baseline.Distance);
		}

		public void Deserialize(in BitBuffer buffer, in UnitEnemySeekingStateSnapshot baseline, in GhostSetup setup)
		{
			Enemy    = buffer.ReadGhostDelta(baseline.Enemy);
			Distance = buffer.ReadIntDelta(baseline.Distance);
		}

		public void FromComponent(in UnitEnemySeekingState component, in GhostSetup setup)
		{
			Enemy    = setup.ToGhost(component.Enemy);
			Distance = (int) (component.Distance * 15);
		}

		public void ToComponent(ref UnitEnemySeekingState component, in GhostSetup setup)
		{
			component.Enemy    = setup.FromGhost(Enemy);
			component.Distance = (float) (Distance / 15f);
		}
	}
}