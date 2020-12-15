using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.Roles.Interfaces
{
	/// <summary>
	/// An entity description is a role attributed to a <see cref="GameEntity"/>
	/// </summary>
	public interface IEntityDescription : IComponentData
	{
		public class Serializer<T> : ArchetypeOnlySerializerBase<T>
			where T : struct, IEntityDescription
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}
}