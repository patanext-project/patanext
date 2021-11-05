using DefaultEcs;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.Network.Components
{
	public enum WantedTimeType
	{
		Default,
		Interpolated,
		Extrapolated
	}

	public struct AssignInstigatorTime : IReadWriteComponentData<AssignInstigatorTime, EmptySnapshotSetup>
	{
		public WantedTimeType RequestedTimeType;
		public int            Instigator;

		public AssignInstigatorTime(WantedTimeType wantedTimeType, int instigator = 0)
		{
			RequestedTimeType = wantedTimeType;
			Instigator        = 0;
		}

		public bool ShouldBeInterpolated => RequestedTimeType == WantedTimeType.Default || RequestedTimeType == WantedTimeType.Interpolated;
		public bool ShouldBeExtrapolated => RequestedTimeType == WantedTimeType.Extrapolated;

		public class Serializer : DeltaComponentSerializerBase<AssignInstigatorTime>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
		
		public void Serialize(in BitBuffer buffer, in AssignInstigatorTime baseline, in EmptySnapshotSetup setup)
		{
			buffer.AddUIntD4((byte) RequestedTimeType);
			buffer.AddInt(Instigator);
		}

		public void Deserialize(in BitBuffer buffer, in AssignInstigatorTime baseline, in EmptySnapshotSetup setup)
		{
			RequestedTimeType = (WantedTimeType) buffer.ReadUIntD4();
			Instigator        = buffer.ReadInt();
		}
	}
}