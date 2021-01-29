using System;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct GameComboStateSnapshot : IReadWriteSnapshotData<GameComboStateSnapshot>, ISnapshotSyncWithComponent<GameCombo.State>
	{
		public class Serializer : DeltaSnapshotSerializerBase<GameComboStateSnapshot, GameCombo.State>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings              = false;
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}

			protected override IAuthorityArchetype GetAuthorityArchetype()
			{
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public uint Tick { get; set; }

		public int  Count;
		public uint Score;

		public void Serialize(in BitBuffer buffer, in GameComboStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(Count, baseline.Count)
			      .AddUIntD4Delta(Score, baseline.Score);
		}

		public void Deserialize(in BitBuffer buffer, in GameComboStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			Count = buffer.ReadIntDelta(baseline.Count);
			Score = buffer.ReadUIntD4Delta(baseline.Score);
		}

		public void FromComponent(in GameCombo.State component, in EmptySnapshotSetup setup)
		{
			Count = component.Count;
			Score = (uint) (component.Score * 100);
		}

		public void ToComponent(ref GameCombo.State component, in EmptySnapshotSetup setup)
		{
			component.Count = Count;
			component.Score = Score * 0.01f;
		}
	}

	public struct GameComboSettingsSnapshot : IReadWriteSnapshotData<GameComboSettingsSnapshot>, ISnapshotSyncWithComponent<GameCombo.Settings>
	{
		public class Serializer : DeltaSnapshotSerializerBase<GameComboSettingsSnapshot, GameCombo.Settings>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings              = false;
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}
		}

		public uint Tick { get; set; }

		public int  MaxComboToReachFever;
		public uint RequiredScoreStart;
		public uint RequiredScoreStep;

		public void Serialize(in BitBuffer buffer, in GameComboSettingsSnapshot baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddIntDelta(MaxComboToReachFever, baseline.MaxComboToReachFever)
			      .AddUIntD4Delta(RequiredScoreStart, baseline.RequiredScoreStart)
			      .AddUIntD4Delta(RequiredScoreStep, baseline.RequiredScoreStep);
		}

		public void Deserialize(in BitBuffer buffer, in GameComboSettingsSnapshot baseline, in EmptySnapshotSetup setup)
		{
			MaxComboToReachFever = buffer.ReadIntDelta(baseline.MaxComboToReachFever);
			RequiredScoreStart   = buffer.ReadUIntD4Delta(baseline.RequiredScoreStart);
			RequiredScoreStep    = buffer.ReadUIntD4Delta(baseline.RequiredScoreStep);
		}

		public void FromComponent(in GameCombo.Settings component, in EmptySnapshotSetup setup)
		{
			MaxComboToReachFever = component.MaxComboToReachFever;
			RequiredScoreStart   = (uint) (component.RequiredScoreStart * 100);
			RequiredScoreStep    = (uint) (component.RequiredScoreStep * 100);
		}

		public void ToComponent(ref GameCombo.Settings component, in EmptySnapshotSetup setup)
		{
			component.MaxComboToReachFever = MaxComboToReachFever;
			component.RequiredScoreStart   = RequiredScoreStart * 0.01f;
			component.RequiredScoreStep    = RequiredScoreStep * 0.01f;
		}
	}
}