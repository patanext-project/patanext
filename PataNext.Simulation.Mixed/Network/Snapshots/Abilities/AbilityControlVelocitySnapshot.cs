using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;

namespace PataNext.Module.Simulation.Network.Snapshots.Abilities
{
	public struct AbilityControlVelocitySnapshot
	{
		public class Serializer : ArchetypeOnlySerializerBase<AbilityControlVelocity>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}
}