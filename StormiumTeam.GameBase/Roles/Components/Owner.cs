using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.Roles.Components
{
	public readonly struct Owner : IComponentData
	{
		public readonly GameEntity Target;

		public Owner(GameEntity target)
		{
			Target = target;
		}

		public class Register : RegisterGameHostComponentData<Owner>
		{
		}

		public class Serializer : DeltaSnapshotSerializerBase<Snapshot, Owner, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public struct Snapshot : IReadWriteSnapshotData<Snapshot, GhostSetup>, ISnapshotSyncWithComponent<Owner, GhostSetup>
		{
			public uint Tick { get; set; }

			public Ghost Ghost;

			public void Serialize(in BitBuffer buffer, in Snapshot baseline, in GhostSetup setup)
			{
				buffer.AddGhostDelta(Ghost, baseline.Ghost);
			}

			public void Deserialize(in BitBuffer buffer, in Snapshot baseline, in GhostSetup setup)
			{
				Ghost = buffer.ReadGhostDelta(baseline.Ghost);
			}

			public void FromComponent(in Owner component, in GhostSetup setup)
			{
				Ghost = setup.ToGhost(component.Target);
			}

			// ReSharper disable RedundantAssignment
			public void ToComponent(ref Owner component, in GhostSetup setup)
			{
				component = new Owner(setup.FromGhost(Ghost));
			}
			// ReSharper restore RedundantAssignment
		}
	}
}