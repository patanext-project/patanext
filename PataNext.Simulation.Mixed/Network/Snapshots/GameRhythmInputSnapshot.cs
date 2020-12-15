using System;
using System.Runtime.CompilerServices;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Utility.InterTick;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components;
using RevolutionSnapshot.Core.Buffers;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public unsafe struct GameRhythmInputSnapshot : IReadWriteSnapshotData<GameRhythmInputSnapshot>, ISnapshotSyncWithComponent<GameRhythmInputComponent>
	{
		public class Serializer : DeltaComponentSerializerBase<GameRhythmInputSnapshot, GameRhythmInputComponent>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				DirectComponentSettings = true;
			}

			protected override IAuthorityArchetype GetAuthorityArchetype()
			{
				return AuthoritySerializer<InputAuthority>.CreateAuthorityArchetype(GameWorld);
			}
		}

		public uint Tick { get; set; }

		public uint                  Panning;
		public InterFramePressAction AbilityInterFrame;
		public uint                  Ability;

		public static readonly BoundedRange BoundedRange = new(-1, 1, 0.1f);

		public void Serialize(in BitBuffer buffer, in GameRhythmInputSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (UnsafeUtility.SameData(this, baseline))
			{
				buffer.AddBool(false);
				return;
			}

			buffer.AddBool(true)
			      .AddUIntD4Delta(Panning, baseline.Panning)
			      .AddUIntD4Delta(Ability, baseline.Ability)
			      .AddInterFrameDelta(AbilityInterFrame, baseline.AbilityInterFrame);

			var span         = Actions;
			var baselineSpan = baseline.Actions;
			for (var i = 0; i < span.Length; i++)
			{
				buffer.AddInterFrameDelta(span[i].InterFrame, baselineSpan[i].InterFrame)
				      .AddBool(span[i].IsActive)
				      .AddBool(span[i].IsSliding);
			}
		}

		public void Deserialize(in BitBuffer buffer, in GameRhythmInputSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			Panning           = buffer.ReadUIntD4Delta(baseline.Panning);
			Ability           = buffer.ReadUIntD4Delta(baseline.Ability);
			AbilityInterFrame = buffer.ReadInterFrameDelta(baseline.AbilityInterFrame);

			var span         = Actions;
			var baselineSpan = baseline.Actions;
			for (var i = 0; i < span.Length; i++)
			{
				span[i] = new GameRhythmInputComponent.RhythmAction
				{
					InterFrame = buffer.ReadInterFrameDelta(baselineSpan[i].InterFrame),
					IsActive   = buffer.ReadBool(),
					IsSliding  = buffer.ReadBool()
				};
			}
		}

		public void FromComponent(in GameRhythmInputComponent component, in EmptySnapshotSetup setup)
		{
			Panning           = BoundedRange.Quantize(component.Panning);
			Ability           = (uint) component.Ability;
			AbilityInterFrame = component.AbilityInterFrame;
			for (var i = 0; i < component.Actions.Length; i++)
				Actions[i] = component.Actions[i];
		}

		public void ToComponent(ref GameRhythmInputComponent component, in EmptySnapshotSetup setup)
		{
			component.Panning           = BoundedRange.Dequantize(Panning);
			component.Ability           = (AbilitySelection) Ability;
			component.AbilityInterFrame = AbilityInterFrame;
			for (var i = 0; i < component.Actions.Length; i++)
				component.Actions[i] = Actions[i];
		}

		private GameRhythmInputComponent.RhythmAction action0;
		private GameRhythmInputComponent.RhythmAction action1;
		private GameRhythmInputComponent.RhythmAction action2;
		private GameRhythmInputComponent.RhythmAction action3;

		public Span<GameRhythmInputComponent.RhythmAction> Actions
		{
			get
			{
				fixed (GameRhythmInputComponent.RhythmAction* fixedPtr = &action0)
				{
					return new Span<GameRhythmInputComponent.RhythmAction>(fixedPtr, 4);
				}
			}
		}
	}
}