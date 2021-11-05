using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace StormiumTeam.GameBase.Roles.Components
{
	public readonly struct OwnedRelative<T> : IComponentBuffer
		where T : IEntityDescription
	{
		public readonly GameEntity Target;

		public OwnedRelative(GameEntity entity)
		{
			Target = entity;
		}

		public class Serializer : ArchetypeOnlySerializerBase<OwnedRelative<T>>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}
}