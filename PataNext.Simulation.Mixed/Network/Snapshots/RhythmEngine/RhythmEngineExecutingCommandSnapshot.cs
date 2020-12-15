using System;
using System.Diagnostics.CodeAnalysis;
using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Systems.Instigators;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Resources;
using RevolutionSnapshot.Core.Buffers;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct RhythmEngineExecutingCommandSnapshot : IReadWriteSnapshotData<RhythmEngineExecutingCommandSnapshot, GhostSetup>,
	                                                     ISnapshotSyncWithComponent<RhythmEngineExecutingCommand, GhostSetup>
	{
		public class Serializer : DeltaComponentSerializerBase<RhythmEngineExecutingCommandSnapshot, RhythmEngineExecutingCommand, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}

			protected override IAuthorityArchetype? GetAuthorityArchetype()
			{
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public uint Tick { get; set; }

		private bool  WaitingForApply;
		private int   ActivationBeatStart;
		private int   ActivationBeatEnd;
		private Ghost CommandTarget;
		private Ghost Previous;
		private int   PowerInteger;

		public void Serialize(in BitBuffer buffer, in RhythmEngineExecutingCommandSnapshot baseline, in GhostSetup setup)
		{
			var areSame = UnsafeUtility.SameData(this, baseline);
			buffer.AddBool(areSame);
			if (areSame)
				return;

			buffer.AddBool(WaitingForApply)
			      .AddIntDelta(ActivationBeatStart, baseline.ActivationBeatStart)
			      .AddIntDelta(ActivationBeatEnd, baseline.ActivationBeatEnd)
			      .AddGhostDelta(CommandTarget, baseline.CommandTarget)
			      .AddGhostDelta(Previous, baseline.Previous)
			      .AddIntDelta(PowerInteger, baseline.PowerInteger);
		}

		public void Deserialize(in BitBuffer buffer, in RhythmEngineExecutingCommandSnapshot baseline, in GhostSetup setup)
		{
			var areSame = buffer.ReadBool();
			if (areSame)
			{
				this = baseline;
				return;
			}

			WaitingForApply     = buffer.ReadBool();
			ActivationBeatStart = buffer.ReadIntDelta(baseline.ActivationBeatStart);
			ActivationBeatEnd   = buffer.ReadIntDelta(baseline.ActivationBeatEnd);
			CommandTarget       = buffer.ReadGhostDelta(baseline.CommandTarget);
			Previous            = buffer.ReadGhostDelta(baseline.Previous);
			PowerInteger        = buffer.ReadIntDelta(baseline.PowerInteger);
		}

		public void FromComponent(in RhythmEngineExecutingCommand component, in GhostSetup setup)
		{
			WaitingForApply     = component.WaitingForApply;
			ActivationBeatStart = component.ActivationBeatStart;
			ActivationBeatEnd   = component.ActivationBeatEnd;
			CommandTarget       = setup.ToGhost(component.CommandTarget.Entity);
			Previous            = setup.ToGhost(component.Previous.Entity);
			PowerInteger        = component.PowerInteger;
		}

		public void ToComponent(ref RhythmEngineExecutingCommand component, in GhostSetup setup)
		{
			component.WaitingForApply     = WaitingForApply;
			component.ActivationBeatStart = ActivationBeatStart;
			component.ActivationBeatEnd   = ActivationBeatEnd;
			component.CommandTarget       = new GameResource<RhythmCommandResource>(setup.FromGhost(CommandTarget));
			component.Previous            = new GameResource<RhythmCommandResource>(setup.FromGhost(Previous));
			component.PowerInteger        = PowerInteger;
		}
	}
}