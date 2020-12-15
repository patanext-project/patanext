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

		public class Serializer : DeltaComponentSerializerBase<Snapshot, Owner, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public struct Snapshot : IReadWriteSnapshotData<Snapshot, GhostSetup>, ISnapshotSyncWithComponent<Owner, GhostSetup>
		{
			public uint Tick { get; set; }

			public GameEntity Target;

			public void Serialize(in BitBuffer buffer, in Snapshot baseline, in GhostSetup setup)
			{
				buffer.AddUIntD4Delta(Target.Id, baseline.Target.Id)
				      .AddUIntD4Delta(Target.Version, baseline.Target.Version);
			}

			public void Deserialize(in BitBuffer buffer, in Snapshot baseline, in GhostSetup setup)
			{
				Target = new GameEntity(
					buffer.ReadUIntD4Delta(baseline.Target.Id),
					buffer.ReadUIntD4Delta(baseline.Target.Version)
				);
			}

			public void FromComponent(in Owner component, in GhostSetup setup)
			{
				Target = setup[component.Target];
			}

			// ReSharper disable RedundantAssignment
			public void ToComponent(ref Owner component, in GhostSetup setup)
			{
				component = new Owner(setup[Target]);
			}
			// ReSharper restore RedundantAssignment
		}
	}
}