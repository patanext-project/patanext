using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct GroundStateSnapshot : IReadWriteSnapshotData<GroundStateSnapshot>, ISnapshotSyncWithComponent<GroundState>
	{
		public class Serializer : DeltaSnapshotSerializerBase<GroundStateSnapshot, GroundState>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				DirectComponentSettings = true;
			}

			protected override IAuthorityArchetype? GetAuthorityArchetype()
			{
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public uint Tick { get; set; }

		public bool onGround;
		
		public void Serialize(in     BitBuffer   buffer,    in GroundStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddBool(onGround);
		}

		public void Deserialize(in   BitBuffer   buffer,    in GroundStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			onGround = buffer.ReadBool();
		}

		public void FromComponent(in GroundState component, in EmptySnapshotSetup  setup)
		{
			onGround = component.Value;
		}

		public void ToComponent(ref  GroundState component, in EmptySnapshotSetup  setup)
		{
			component.Value = onGround;
		}
	}
}