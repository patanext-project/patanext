using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.Network.Authorities;

namespace StormiumTeam.GameBase.Physics.Components
{
	public struct Velocity : IComponentData
	{
		public Vector3 Value;
		
		public class Register : RegisterGameHostComponentData<Velocity>
		{}
		
		public class Serializer : DeltaComponentSerializerBase<Snapshot, Velocity>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}

			protected override IAuthorityArchetype? GetAuthorityArchetype()
			{
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public struct Snapshot : IReadWriteSnapshotData<Snapshot>, ISnapshotSyncWithComponent<Velocity>
		{
			public uint Tick { get; set; }

			public uint X, Y, Z;

			private static readonly BoundedRange RangeXz = new(-100, 100, 0.01f);
			private static readonly BoundedRange RangeY  = new(-100, 100, 0.01f);

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

			public void FromComponent(in Velocity component, in EmptySnapshotSetup setup)
			{
				X = RangeXz.Quantize(component.Value.X);
				Z = RangeXz.Quantize(component.Value.Z);
				Y = RangeY.Quantize(component.Value.Y);
			}

			public void ToComponent(ref Velocity component, in EmptySnapshotSetup setup)
			{
				component.Value.X = RangeXz.Dequantize(X);
				component.Value.Z = RangeXz.Dequantize(Z);
				component.Value.Y = RangeY.Dequantize(Y);
			}
		}
	}
}