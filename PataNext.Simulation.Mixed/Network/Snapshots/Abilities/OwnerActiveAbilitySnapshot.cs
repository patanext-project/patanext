using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;

namespace PataNext.Module.Simulation.Network.Snapshots.Abilities
{
	public struct OwnerActiveAbilitySnapshot
	{
		public class Serializer : ArchetypeOnlySerializerBase<OwnerActiveAbility>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}
}