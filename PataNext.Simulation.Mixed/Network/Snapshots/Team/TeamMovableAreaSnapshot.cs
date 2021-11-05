using System.Runtime.InteropServices;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots.Team
{
	public struct TeamMovableAreaSnapshot : IReadWriteSnapshotData<TeamMovableAreaSnapshot>, ISnapshotSyncWithComponent<TeamMovableArea>
	{
		public class Serializer : DeltaSnapshotSerializerBase<TeamMovableAreaSnapshot, TeamMovableArea>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings              = false;
				CheckDifferenceSettings          = true;
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}

			protected override IAuthorityArchetype? GetAuthorityArchetype()
			{
				// TODO: Custom authority for TeamMovableArea component?
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public uint Tick { get; set; }

		[StructLayout(LayoutKind.Explicit)]
		private struct Union
		{
			[FieldOffset(0)] public int   Int;
			[FieldOffset(0)] public float Float;
		}

		public int Left, Right;

		public void Serialize(in BitBuffer buffer, in TeamMovableAreaSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(Left, baseline.Left);
			buffer.AddIntDelta(Right, baseline.Right);
		}

		public void Deserialize(in BitBuffer buffer, in TeamMovableAreaSnapshot baseline, in EmptySnapshotSetup setup)
		{
			Left  = buffer.ReadIntDelta(baseline.Left);
			Right = buffer.ReadIntDelta(baseline.Right);
		}

		public void FromComponent(in TeamMovableArea component, in EmptySnapshotSetup setup)
		{
			Left  = new Union {Float = component.Left}.Int;
			Right = new Union {Float = component.Right}.Int;
		}

		public void ToComponent(ref TeamMovableArea component, in EmptySnapshotSetup setup)
		{
			component.Left  = new Union {Int = Left}.Float;
			component.Right = new Union {Int = Right}.Float;
		}
	}
}