using System;
using GameHost.Injection;
using GameHost.Native.Char;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Setups;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Resources;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Module.Simulation.Network.Snapshots.Resources
{
	public struct RhythmCommandResourceSnapshot : IReadWriteSnapshotData<RhythmCommandResourceSnapshot, GetSerializerSetup>,
	                                              ISnapshotSyncWithComponent<RhythmCommandResource, GetSerializerSetup>
	{
		public class Serializer : DeltaSnapshotSerializerBase<RhythmCommandResourceSnapshot, RhythmCommandResource, GetSerializerSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}
		}

		public uint Tick { get; set; }

		public CharBuffer128 Identifier;

		public void Serialize(in BitBuffer buffer, in RhythmCommandResourceSnapshot baseline, in GetSerializerSetup setup)
		{
			buffer.AddString(Identifier.ToString());
		}

		public void Deserialize(in BitBuffer buffer, in RhythmCommandResourceSnapshot baseline, in GetSerializerSetup setup)
		{
			Identifier = CharBufferUtility.Create<CharBuffer128>(buffer.ReadString());
		}

		public void FromComponent(in RhythmCommandResource component, in GetSerializerSetup setup)
		{
			Identifier = setup.GameWorld
			                  .Boards
			                  .ComponentType
			                  .NameColumns[(int) component.Identifier.Id];
		}

		public void ToComponent(ref RhythmCommandResource component, in GetSerializerSetup setup)
		{
			var nameColumns = setup.GameWorld.Boards.ComponentType.NameColumns;
			for (var i = 1; i < nameColumns.Length; i++)
			{
				if (nameColumns[i].AsSpan().SequenceEqual(Identifier.Span))
				{
					component = new RhythmCommandResource(new ComponentType((uint) i));
					break;
				}
			}
		}
	}

	public struct RhythmCommandIdentifierSnapshot : IReadWriteSnapshotData<RhythmCommandIdentifierSnapshot, GetSerializerSetup>,
	                                                ISnapshotSyncWithComponent<RhythmCommandIdentifier, GetSerializerSetup>
	{
		public class Serializer : DeltaSnapshotSerializerBase<RhythmCommandIdentifierSnapshot, RhythmCommandIdentifier, GetSerializerSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				CheckEqualsWholeSnapshotSettings = EqualsWholeSnapshot.CheckWithComponentDifference;
			}
		}

		public uint Tick { get; set; }

		public CharBuffer64 Identifier;

		public void Serialize(in BitBuffer buffer, in RhythmCommandIdentifierSnapshot baseline, in GetSerializerSetup setup)
		{
			buffer.AddString(Identifier.ToString());
		}

		public void Deserialize(in BitBuffer buffer, in RhythmCommandIdentifierSnapshot baseline, in GetSerializerSetup setup)
		{
			Identifier = CharBufferUtility.Create<CharBuffer64>(buffer.ReadString());
		}

		public void FromComponent(in RhythmCommandIdentifier component, in GetSerializerSetup setup)
		{
			Identifier = component.Value;
		}

		public void ToComponent(ref RhythmCommandIdentifier component, in GetSerializerSetup setup)
		{
			component = new RhythmCommandIdentifier(Identifier);
		}
	}
}