using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Module.Simulation.Network.Snapshots.Abilities
{
	// TODO: Optimize data
	public struct AbilityModifyStatsOnChainingSnapshot : IReadWriteSnapshotData<AbilityModifyStatsOnChainingSnapshot>, ISnapshotSyncWithComponent<AbilityModifyStatsOnChaining>
	{
		public class Serializer : DeltaSnapshotSerializerBase<AbilityModifyStatsOnChainingSnapshot, AbilityModifyStatsOnChaining>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings     = false;
				DirectComponentSettings = true;
			}
		}
		
		public uint Tick { get; set; }

		public AbilityModifyStatsOnChaining Value;
		
		public void Serialize(in     BitBuffer                    buffer,    in AbilityModifyStatsOnChainingSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (UnsafeUtility.SameData(this, baseline))
			{
				buffer.AddBool(false);
				return;
			}

			buffer.AddBool(true);
			buffer.AddSpan(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Value, 1)));
		}

		public void Deserialize(in BitBuffer buffer, in AbilityModifyStatsOnChainingSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			var span = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Value, 1));
			buffer.ReadSpan(span, span.Length);
		}

		public void FromComponent(in AbilityModifyStatsOnChaining component, in EmptySnapshotSetup                   setup)
		{
			Value = component;
		}

		public void ToComponent(ref AbilityModifyStatsOnChaining component, in EmptySnapshotSetup setup)
		{
			component = Value;
		}
	}
}