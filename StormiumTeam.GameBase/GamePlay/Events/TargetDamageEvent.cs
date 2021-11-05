using System.Runtime.InteropServices;
using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.GamePlay.Events
{
	public struct TargetDamageEvent : IComponentData
	{
		public GameEntity Instigator;
		public GameEntity Victim;

		public double Damage;

		public TargetDamageEvent(GameEntity instigator, GameEntity victim, double damage)
		{
			Instigator = instigator;
			Victim     = victim;

			Damage = damage;
		}

		public class Serializer : DeltaSnapshotSerializerBase<Snapshot, TargetDamageEvent, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
		}

		public struct Snapshot : IReadWriteSnapshotData<Snapshot, GhostSetup>, ISnapshotSyncWithComponent<TargetDamageEvent, GhostSetup>
		{
			[StructLayout(LayoutKind.Explicit)]
			private struct Union
			{
				[FieldOffset(0)] public long   Long;
				[FieldOffset(0)] public double Double;
			}

			public uint Tick { get; set; }

			public Ghost Instigator, Victim;
			public long  Damage;

			public void Serialize(in BitBuffer buffer, in Snapshot baseline, in GhostSetup setup)
			{
				buffer.AddGhostDelta(Instigator, baseline.Instigator);
				buffer.AddGhostDelta(Victim, baseline.Victim);
				buffer.AddLongDelta(Damage, baseline.Damage);
			}

			public void Deserialize(in BitBuffer buffer, in Snapshot baseline, in GhostSetup setup)
			{
				Instigator = buffer.ReadGhostDelta(baseline.Instigator);
				Victim     = buffer.ReadGhostDelta(baseline.Victim);
				Damage     = buffer.ReadLongDelta(baseline.Damage);
			}

			public void FromComponent(in TargetDamageEvent component, in GhostSetup setup)
			{
				Instigator = setup.ToGhost(component.Instigator);
				Victim     = setup.ToGhost(component.Victim);
				Damage     = new Union {Double = component.Damage}.Long;
			}

			public void ToComponent(ref TargetDamageEvent component, in GhostSetup setup)
			{
				component.Instigator = setup.FromGhost(Instigator);
				component.Victim     = setup.FromGhost(Victim);
				component.Damage     = new Union {Long = Damage}.Double;
			}
		}
	}
}