using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Units;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct UnitEnemySeekingStateSnapshot
	{
		public class Serializer : ArchetypeOnlySerializerBase<UnitEnemySeekingState>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}
}