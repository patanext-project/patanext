using System;
using System.Runtime.InteropServices;
using DefaultEcs;
using GameHost.Injection;
using GameHost.Revolution.NetCode.Components;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Time;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.Network
{
	public struct ForeignNetTime : IComponentData
	{
		public (GameTime begin, GameTime end) ClientTime;
		public GameTime                       LastAcknowledgedServerTime;

		public TimeSpan GetLatency(TimeSpan serverTime) => serverTime - TimeSpan.FromSeconds(LastAcknowledgedServerTime.Elapsed);
	}

	public class NetForeignTimeSerializer : SerializerBase
	{
		public struct Core : IComponentData
		{
		}

		public NetForeignTimeSerializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context context) : base(instigator, context)
		{
		}

		public override void UpdateMergeGroup(ReadOnlySpan<Entity> clients, MergeGroupCollection collection)
		{
			foreach (var client in clients)
			{
				if (!collection.TryGetGroup(client, out var group))
					group = collection.CreateGroup();

				foreach (var other in clients)
				{
					collection.SetToGroup(other, group);
				}
			}
		}

		public override void OnReset(ISnapshotInstigator instigator)
		{

		}

		protected override ISerializerArchetype GetSerializerArchetype()
		{
			return new SimpleSerializerArchetype(this, GameWorld,
				GameWorld.AsComponentType<Core>(),
				new[] {GameWorld.AsComponentType<ForeignNetTime>()},
				Array.Empty<ComponentType>());
		}

		protected override void OnSerialize(BitBuffer bitBuffer, SerializationParameters parameters, MergeGroup @group, ReadOnlySpan<GameEntityHandle> entities)
		{
			var accessor = new ComponentDataAccessor<ForeignNetTime>(GameWorld);

			ForeignNetTime previous = default;
			foreach (var entity in entities)
			{
				var current = accessor[entity];

				static void addGt(BitBuffer buffer, GameTime curr, GameTime prev)
				{
					buffer.AddUIntDelta(Union.IntToUInt(curr.Frame), Union.IntToUInt(prev.Frame));
					buffer.AddUIntDelta(Union.FloatToUInt(curr.Delta), Union.FloatToUInt(prev.Delta));

					var tupleCurr = Union.DoubleToUInt(curr.Delta);
					var tuplePrev = Union.DoubleToUInt(prev.Delta);
					buffer.AddUIntDelta(tupleCurr.Item1, tuplePrev.Item1);
					buffer.AddUIntDelta(tupleCurr.Item2, tupleCurr.Item1);
				}

				addGt(bitBuffer, current.ClientTime.begin, previous.ClientTime.begin);
				addGt(bitBuffer, current.ClientTime.end, previous.ClientTime.end);
				addGt(bitBuffer, current.LastAcknowledgedServerTime, previous.LastAcknowledgedServerTime);

				previous = current;
			}
		}

		protected override void OnDeserialize(BitBuffer bitBuffer, DeserializationParameters parameters, ISnapshotSerializerSystem.RefData refData)
		{
			var accessor = new ComponentDataAccessor<ForeignNetTime>(GameWorld);

			ForeignNetTime previous = default;
			for (var ent = 0; ent < refData.Self.Length; ent++)
			{
				var self     = refData.Self[ent];
				var snapshot = refData.Snapshot[ent];

				static void readGt(BitBuffer buffer, out GameTime nxt, GameTime prev)
				{
					nxt.Frame = Union.UIntToInt(buffer.ReadUIntDelta(Union.IntToUInt(prev.Frame)));
					nxt.Delta = Union.UIntToFloat(buffer.ReadUIntDelta(Union.FloatToUInt(prev.Delta)));

					var          tuplePrev = Union.DoubleToUInt(prev.Delta);
					(uint, uint) tupleNext;
					tupleNext.Item1 = buffer.ReadUIntDelta(tuplePrev.Item1);
					tupleNext.Item2 = buffer.ReadUIntDelta(tupleNext.Item1);

					nxt.Elapsed = Union.UIntToDouble(tupleNext);
				}

				ForeignNetTime next;
				readGt(bitBuffer, out next.ClientTime.begin, previous.ClientTime.begin);
				readGt(bitBuffer, out next.ClientTime.end, previous.ClientTime.end);
				readGt(bitBuffer, out next.LastAcknowledgedServerTime, previous.LastAcknowledgedServerTime);
				previous = next;

				if (refData.IgnoredSet[(int) snapshot.Id])
					continue;

				accessor[self] = next;
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct Union
		{
			[FieldOffset(0)] private int    Int32;
			[FieldOffset(0)] private uint   UInt32_1;
			[FieldOffset(4)] private uint   UInt32_2;
			[FieldOffset(0)] private double Double64;
			[FieldOffset(0)] private float  Float32;

			public static (uint, uint) DoubleToUInt(double val)
			{
				Union union = default;
				union.Double64 = val;
				return (union.UInt32_1, union.UInt32_2);
			}

			public static double UIntToDouble((uint, uint) tuple)
			{
				Union union = default;
				union.UInt32_1 = tuple.Item1;
				union.UInt32_2 = tuple.Item2;
				return union.Double64;
			}

			public static uint IntToUInt(int val)
			{
				Union union = default;
				union.Int32 = val;
				return union.UInt32_1;
			}

			public static int UIntToInt(uint val)
			{
				Union union = default;
				union.UInt32_1 = val;
				return union.Int32;
			}

			public static uint FloatToUInt(float val)
			{
				Union union = default;
				union.Float32 = val;
				return union.UInt32_1;
			}

			public static float UIntToFloat(uint val)
			{
				Union union = default;
				union.UInt32_1 = val;
				return union.Float32;
			}
		}
	}
}