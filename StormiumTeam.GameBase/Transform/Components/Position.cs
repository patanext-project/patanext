using System;
using System.Numerics;
using BepuUtilities;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.Transform.Components
{
	public struct Position : IComponentData
	{
		public Vector3 Value;
		
		public Position(Vector3 val)
		{
			Value = val;
		}

		public Position(float x = 0, float y = 0, float z = 0)
		{
			Value.X = x;
			Value.Y = y;
			Value.Z = z;
		}

		public class Serializer : DeltaComponentSerializerBase<Snapshot, Position>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public struct Snapshot : IReadWriteSnapshotData<Snapshot>, ISnapshotSyncWithComponent<Position>
		{
			public uint Tick { get; set; }

			public uint X, Y, Z;

			private static readonly BoundedRange RangeXz = new(-2000, 2000, 0.02f);
			private static readonly BoundedRange RangeY  = new(-500, 500, 0.02f);

			public void Serialize(in BitBuffer buffer, in Snapshot baseline, in EmptySnapshotSetup setup)
			{
				buffer.AddUIntD4Delta(X, baseline.X)
				      .AddUIntD4Delta(Y, baseline.Y)
				      .AddUIntD4Delta(Z, baseline.Z);
			}

			public void Deserialize(in BitBuffer buffer, in Snapshot baseline, in EmptySnapshotSetup setup)
			{
				X = buffer.ReadUIntD4Delta(baseline.X);
				Y = buffer.ReadUIntD4Delta(baseline.Y);
				Z = buffer.ReadUIntD4Delta(baseline.Z);
			}

			public void FromComponent(in Position component, in EmptySnapshotSetup setup)
			{
				X = RangeXz.Quantize(component.Value.X);
				Z = RangeXz.Quantize(component.Value.Z);
				Y = RangeY.Quantize(component.Value.Y);
			}

			public void ToComponent(ref Position component, in EmptySnapshotSetup setup)
			{
				component.Value.X = RangeXz.Dequantize(X);
				component.Value.Z = RangeXz.Dequantize(Z);
				component.Value.Y = RangeY.Dequantize(Y);
			}
		}
	}
}