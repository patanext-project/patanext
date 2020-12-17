using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Simulation.Utility.Resource.Components;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.Network
{
	public class IsResourceEntitySerializer : ArchetypeOnlySerializerBase<IsResourceEntity>
	{
		public IsResourceEntitySerializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
		{
		}
	}
}