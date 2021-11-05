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
		public int   SelfDistance;
		public int   RelativeDistance;

		public void Serialize(in BitBuffer buffer, in UnitEnemySeekingStateSnapshot baseline, in GhostSetup setup)
		{
			buffer.AddGhostDelta(Enemy, baseline.Enemy);
			buffer.AddIntDelta(SelfDistance, baseline.SelfDistance);
			buffer.AddIntDelta(RelativeDistance, baseline.RelativeDistance);
		}

		public void Deserialize(in BitBuffer buffer, in UnitEnemySeekingStateSnapshot baseline, in GhostSetup setup)
		{
			Enemy            = buffer.ReadGhostDelta(baseline.Enemy);
			SelfDistance     = buffer.ReadIntDelta(baseline.SelfDistance);
			RelativeDistance = buffer.ReadIntDelta(baseline.RelativeDistance);
		}

		public void FromComponent(in UnitEnemySeekingState component, in GhostSetup setup)
		{
			Enemy            = setup.ToGhost(component.Enemy);
			SelfDistance     = (int) (component.SelfDistance * 15);
			RelativeDistance = (int) (component.RelativeDistance * 15);
		}

		public void ToComponent(ref UnitEnemySeekingState component, in GhostSetup setup)
		{
			component.Enemy            = setup.FromGhost(Enemy);
			component.SelfDistance     = (float) (SelfDistance / 15f);
			component.RelativeDistance = (float) (RelativeDistance / 15f);
		}
	}
}