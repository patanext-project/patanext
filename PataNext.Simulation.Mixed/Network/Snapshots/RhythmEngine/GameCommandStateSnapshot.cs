using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Network.Authorities;


namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct GameCommandStateSnapshot : IReadWriteSnapshotData<GameCommandStateSnapshot>, ISnapshotSyncWithComponent<GameCommandState>
	{
		public class Serializer : DeltaSnapshotSerializerBase<GameCommandStateSnapshot, GameCommandState>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
			
			protected override IAuthorityArchetype GetAuthorityArchetype()
			{
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public uint Tick { get; set; }

		public uint Selection;
		public int  StartTimeMs;
		public int  EndTimeMs;
		public int  ChainEndTimeMs;

		public void Serialize(in BitBuffer buffer, in GameCommandStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(StartTimeMs, baseline.StartTimeMs)
			      .AddIntDelta(EndTimeMs, baseline.EndTimeMs)
			      .AddIntDelta(ChainEndTimeMs, baseline.ChainEndTimeMs)
			      .AddUIntD4Delta(Selection, baseline.Selection);
		}

		public void Deserialize(in BitBuffer buffer, in GameCommandStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			StartTimeMs    = buffer.ReadIntDelta(baseline.StartTimeMs);
			EndTimeMs      = buffer.ReadIntDelta(baseline.EndTimeMs);
			ChainEndTimeMs = buffer.ReadIntDelta(baseline.ChainEndTimeMs);
			Selection      = buffer.ReadUIntD4Delta(baseline.Selection);
		}

		public void FromComponent(in GameCommandState component, in EmptySnapshotSetup setup)
		{
			StartTimeMs    = component.StartTimeMs;
			EndTimeMs      = component.EndTimeMs;
			ChainEndTimeMs = component.ChainEndTimeMs;
			Selection      = (byte) component.Selection;
		}

		public void ToComponent(ref GameCommandState component, in EmptySnapshotSetup setup)
		{
			component.StartTimeMs    = StartTimeMs;
			component.EndTimeMs      = EndTimeMs;
			component.ChainEndTimeMs = ChainEndTimeMs;
			component.Selection      = (AbilitySelection) Selection;
		}
	}
}