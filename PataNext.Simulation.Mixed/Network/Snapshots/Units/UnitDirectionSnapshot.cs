using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.Units;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct UnitDirectionSnapshot : IReadWriteSnapshotData<UnitDirectionSnapshot>, ISnapshotSyncWithComponent<UnitDirection>
	{
		public class Serializer : DeltaSnapshotSerializerBase<UnitDirectionSnapshot, UnitDirection>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}

			protected override IAuthorityArchetype? GetAuthorityArchetype()
			{
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}
		
		public uint Tick { get; set; }

		public bool IsRight;
		
		public void Serialize(in     BitBuffer     buffer,    in UnitDirectionSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddBool(IsRight);
		}

		public void Deserialize(in   BitBuffer     buffer,    in UnitDirectionSnapshot baseline, in EmptySnapshotSetup setup)
		{
			IsRight = buffer.ReadBool();
		}

		public void FromComponent(in UnitDirection component, in EmptySnapshotSetup    setup)
		{
			IsRight = component.IsRight;
		}

		public void ToComponent(ref  UnitDirection component, in EmptySnapshotSetup    setup)
		{
			component = IsRight ? UnitDirection.Right : UnitDirection.Left;
		}
	}
}