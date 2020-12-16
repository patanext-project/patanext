using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using RevolutionSnapshot.Core.Buffers;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public struct UnitStatisticSnapshot : IReadWriteSnapshotData<UnitStatisticSnapshot>, ISnapshotSyncWithComponent<UnitStatistics>
	{
		public class Serializer : DeltaSnapshotSerializerBase<UnitStatisticSnapshot, UnitStatistics>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
		
		public uint Tick { get; set; }

		public int Health;

		public int Attack;
		public int AttackSpeed;

		public int Defense;

		public int MovementAttackSpeed;
		public int BaseWalkSpeed;
		public int FeverWalkSpeed;

		public int Weight;

		public int AttackMeleeRange;
		public int AttackSeekRange;

		public void Serialize(in BitBuffer buffer, in UnitStatisticSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (UnsafeUtility.SameData(this, baseline))
			{
				buffer.AddBool(false);
				return;
			}

			buffer.AddBool(true)
			      .AddIntDelta(Health, baseline.Health)
			      .AddIntDelta(Attack, baseline.Attack)
			      .AddIntDelta(AttackSpeed, baseline.AttackSpeed)

			      .AddIntDelta(Defense, baseline.Defense)

			      .AddIntDelta(MovementAttackSpeed, baseline.MovementAttackSpeed)
			      .AddIntDelta(BaseWalkSpeed, baseline.BaseWalkSpeed)
			      .AddIntDelta(FeverWalkSpeed, baseline.FeverWalkSpeed)

			      .AddIntDelta(Weight, baseline.Weight)

			      .AddIntDelta(AttackMeleeRange, baseline.AttackMeleeRange)
			      .AddIntDelta(AttackSeekRange, baseline.AttackSeekRange);
		}

		public void Deserialize(in BitBuffer buffer, in UnitStatisticSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			Health      = buffer.ReadIntDelta(baseline.Health);
			Attack      = buffer.ReadIntDelta(baseline.Attack);
			AttackSpeed = buffer.ReadIntDelta(baseline.AttackSpeed);

			Defense = buffer.ReadIntDelta(baseline.Defense);

			MovementAttackSpeed = buffer.ReadIntDelta(baseline.MovementAttackSpeed);
			BaseWalkSpeed       = buffer.ReadIntDelta(baseline.BaseWalkSpeed);
			FeverWalkSpeed      = buffer.ReadIntDelta(baseline.FeverWalkSpeed);

			Weight = buffer.ReadIntDelta(baseline.Weight);

			AttackMeleeRange = buffer.ReadIntDelta(baseline.AttackMeleeRange);
			AttackSeekRange  = buffer.ReadIntDelta(baseline.AttackSeekRange);
		}

		public void FromComponent(in UnitStatistics component, in EmptySnapshotSetup setup)
		{
			Health      = component.Health;
			Attack      = component.Attack;
			AttackSpeed = toInt(component.AttackSpeed);

			Defense = component.Defense;

			MovementAttackSpeed = toInt(component.MovementAttackSpeed);
			BaseWalkSpeed       = toInt(component.BaseWalkSpeed);
			FeverWalkSpeed      = toInt(component.FeverWalkSpeed);

			Weight = toInt(component.Weight);

			AttackMeleeRange = toInt(component.AttackMeleeRange);
			AttackSeekRange  = toInt(component.AttackSeekRange);
		}

		public void ToComponent(ref UnitStatistics component, in EmptySnapshotSetup setup)
		{
			component.Health      = Health;
			component.Attack      = Attack;
			component.AttackSpeed = toFloat(AttackSpeed);

			component.Defense = Defense;

			component.MovementAttackSpeed = toFloat(MovementAttackSpeed);
			component.BaseWalkSpeed       = toFloat(BaseWalkSpeed);
			component.FeverWalkSpeed      = toFloat(FeverWalkSpeed);

			component.Weight = toFloat(Weight);

			component.AttackMeleeRange = toFloat(AttackMeleeRange);
			component.AttackSeekRange  = toFloat(AttackSeekRange);
		}

		private static int toInt(float f)
		{
			return (int) f * 1000;
		}

		private static float toFloat(int i)
		{
			return i * 0.001f;
		}
	}

	public struct UnitPlayStateSnapshot : IReadWriteSnapshotData<UnitPlayStateSnapshot>, ISnapshotSyncWithComponent<UnitPlayState>
	{
		public class Serializer : DeltaSnapshotSerializerBase<UnitPlayStateSnapshot, UnitPlayState>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}

			protected override IAuthorityArchetype? GetAuthorityArchetype()
			{
				return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}
		
		public uint Tick { get; set; }
		
		public void Serialize(in     BitBuffer     buffer,    in UnitPlayStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			
		}

		public void Deserialize(in   BitBuffer     buffer,    in UnitPlayStateSnapshot baseline, in EmptySnapshotSetup setup)
		{
			
		}

		public void FromComponent(in UnitPlayState component, in EmptySnapshotSetup    setup)
		{
			
		}

		public void ToComponent(ref  UnitPlayState component, in EmptySnapshotSetup    setup)
		{
			
		}
	}
}