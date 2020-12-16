using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Module.Simulation.Network.Snapshots.Abilities
{
	public struct AbilityActivationSnapshot : IReadWriteSnapshotData<AbilityActivationSnapshot>, ISnapshotSyncWithComponent<AbilityActivation>
	{
		public class Serializer : DeltaSnapshotSerializerBase<AbilityActivationSnapshot, AbilityActivation>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings     = false;
				DirectComponentSettings = true;
			}
		}

		public uint Tick { get; set; }
		
		public uint Type;
		public int  HeroModeMaxCombo;
		public int  HeroModeImperfectLimitBeforeDeactivation;
		public int  DefaultCooldownOnActivation;
		public uint Selection;
		
		public void Serialize(in     BitBuffer         buffer,    in AbilityActivationSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (UnsafeUtility.SameData(this, baseline))
			{
				buffer.AddBool(false);
				return;
			}
			
			buffer.AddBool(true)
			      .AddUIntD4Delta(Type, baseline.Type)
			      .AddIntDelta(HeroModeMaxCombo, baseline.HeroModeMaxCombo)
			      .AddIntDelta(HeroModeImperfectLimitBeforeDeactivation, baseline.HeroModeImperfectLimitBeforeDeactivation)
			      .AddIntDelta(DefaultCooldownOnActivation, baseline.DefaultCooldownOnActivation)
			      .AddUIntD4Delta(Selection, baseline.Selection);
		}

		public void Deserialize(in   BitBuffer         buffer,    in AbilityActivationSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			Type                                     = buffer.ReadUIntD4Delta(baseline.Type);
			HeroModeMaxCombo                         = buffer.ReadIntDelta(baseline.HeroModeMaxCombo);
			HeroModeImperfectLimitBeforeDeactivation = buffer.ReadIntDelta(baseline.HeroModeImperfectLimitBeforeDeactivation);
			DefaultCooldownOnActivation              = buffer.ReadIntDelta(baseline.DefaultCooldownOnActivation);
			Selection                                = buffer.ReadUIntD4Delta(baseline.Selection);
		}

		public void FromComponent(in AbilityActivation component, in EmptySnapshotSetup        setup)
		{
			Type                                     = (uint) component.Type;
			HeroModeMaxCombo                         = component.HeroModeMaxCombo;
			HeroModeImperfectLimitBeforeDeactivation = component.HeroModeImperfectLimitBeforeDeactivation;
			DefaultCooldownOnActivation              = component.DefaultCooldownOnActivation;
			Selection                                = (uint) component.Selection;
		}

		public void ToComponent(ref  AbilityActivation component, in EmptySnapshotSetup        setup)
		{
			component.Type                                     = (EAbilityActivationType) Type;
			component.HeroModeMaxCombo                         = HeroModeMaxCombo;
			component.HeroModeImperfectLimitBeforeDeactivation = HeroModeImperfectLimitBeforeDeactivation;
			component.DefaultCooldownOnActivation              = DefaultCooldownOnActivation;
			component.Selection                                = (AbilitySelection) Selection;
		}
	}
}