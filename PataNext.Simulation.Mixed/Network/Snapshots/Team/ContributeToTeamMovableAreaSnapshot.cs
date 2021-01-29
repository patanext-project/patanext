using System.Runtime.InteropServices;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct MovableAreaAuthority : IComponentData
	{}

	public struct ContributeToTeamMovableAreaSnapshot : IReadWriteSnapshotData<ContributeToTeamMovableAreaSnapshot>, ISnapshotSyncWithComponent<ContributeToTeamMovableArea>
	{
		public class Serializer : DeltaSnapshotSerializerBase<ContributeToTeamMovableAreaSnapshot, ContributeToTeamMovableArea>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings              = false;
				CheckDifferenceSettings          = true;
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}

			protected override IAuthorityArchetype? GetAuthorityArchetype()
			{
				return AuthoritySerializer<MovableAreaAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public uint Tick { get; set; }

		[StructLayout(LayoutKind.Explicit)]
		private struct Union
		{
			[FieldOffset(0)] public int   Int;
			[FieldOffset(0)] public float Float;
		}

		public int Center, Size;

		public void Serialize(in BitBuffer buffer, in ContributeToTeamMovableAreaSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(Center, baseline.Center);
			buffer.AddIntDelta(Size, baseline.Size);
		}

		public void Deserialize(in BitBuffer buffer, in ContributeToTeamMovableAreaSnapshot baseline, in EmptySnapshotSetup setup)
		{
			Center = buffer.ReadIntDelta(baseline.Center);
			Size   = buffer.ReadIntDelta(baseline.Size);
		}

		public void FromComponent(in ContributeToTeamMovableArea component, in EmptySnapshotSetup setup)
		{
			Center = new Union {Float = component.Center}.Int;
			Size   = new Union {Float = component.Size}.Int;
		}

		public void ToComponent(ref ContributeToTeamMovableArea component, in EmptySnapshotSetup setup)
		{
			component = new ContributeToTeamMovableArea(
				center: new Union {Int = Center}.Float,
				size: new Union {Int   = Size}.Float
			);
		}
	}
}