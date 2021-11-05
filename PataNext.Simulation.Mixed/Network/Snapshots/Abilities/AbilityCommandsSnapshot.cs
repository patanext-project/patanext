using GameHost.Injection;
using GameHost.Native;
using GameHost.Native.Fixed;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Utility.Resource;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Resources;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Module.Simulation.Network.Snapshots.Abilities
{
	public struct AbilityCommandsSnapshot : IReadWriteSnapshotData<AbilityCommandsSnapshot, GhostSetup>, ISnapshotSyncWithComponent<AbilityCommands, GhostSetup>
	{
		public class Serializer : DeltaSnapshotSerializerBase<AbilityCommandsSnapshot, AbilityCommands, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
		
		public uint Tick { get; set; }

		public Ghost Chaining;

		// The capacity is increased because of the additional variable 'FromLocal' in Ghost
		public FixedBuffer128<Ghost> Combos;
		public FixedBuffer128<Ghost> HeroModeAllowedCommands;

		public void Serialize(in BitBuffer buffer, in AbilityCommandsSnapshot baseline, in GhostSetup setup)
		{
			if (UnsafeUtility.SameData(this, baseline))
			{
				buffer.AddBool(false);
				return;
			}

			buffer.AddBool(true)
			      .AddGhostDelta(Chaining, baseline.Chaining);
			{
				var comboSpan = Combos.Span;
				buffer.AddUIntD4Delta((uint) comboSpan.Length, (uint) baseline.Combos.GetLength());
				for (var i = 0; i < comboSpan.Length; i++)
				{
					buffer.AddGhostDelta(comboSpan[i], default);
				}
			}
			{
				var heroModeAllowedCommandSpan = HeroModeAllowedCommands.Span;
				buffer.AddUIntD4Delta((uint) heroModeAllowedCommandSpan.Length, (uint) baseline.HeroModeAllowedCommands.GetLength());
				for (var i = 0; i < heroModeAllowedCommandSpan.Length; i++)
				{
					buffer.AddGhostDelta(heroModeAllowedCommandSpan[i], default);
				}
			}
		}

		public void Deserialize(in BitBuffer buffer, in AbilityCommandsSnapshot baseline, in GhostSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			Chaining = buffer.ReadGhostDelta(baseline.Chaining);
			{
				var comboCount = buffer.ReadUIntD4Delta((uint) baseline.Combos.GetLength());
				Combos.SetLength((int) comboCount);
				for (var i = 0; i < comboCount; i++)
				{
					Combos.Span[i] = buffer.ReadGhostDelta(default);
				}
			}
			{
				var heroModeAllowedCommandCount = buffer.ReadUIntD4Delta((uint) baseline.HeroModeAllowedCommands.GetLength());
				HeroModeAllowedCommands.SetLength((int) heroModeAllowedCommandCount);
				for (var i = 0; i < heroModeAllowedCommandCount; i++)
				{
					HeroModeAllowedCommands.Span[i] = buffer.ReadGhostDelta(default);
				}
			}
		}

		public void FromComponent(in AbilityCommands component, in GhostSetup setup)
		{
			Chaining = setup.ToGhost(component.Chaining.Entity);

			Combos.SetLength(component.Combos.GetLength());
			for (var i = 0; i < Combos.GetLength(); i++)
				Combos.Span[i] = setup.ToGhost(component.Combos.Span[i].Entity);

			HeroModeAllowedCommands.SetLength(component.HeroModeAllowedCommands.GetLength());
			for (var i = 0; i < HeroModeAllowedCommands.GetLength(); i++)
				HeroModeAllowedCommands.Span[i] = setup.ToGhost(component.HeroModeAllowedCommands.Span[i].Entity);
		}

		public void ToComponent(ref AbilityCommands component, in GhostSetup setup)
		{
			component.Chaining = new GameResource<RhythmCommandResource>(setup.FromGhost(Chaining));

			component.Combos.SetLength(Combos.GetLength());
			for (var i = 0; i < component.Combos.GetLength(); i++)
				component.Combos.Span[i] = new GameResource<RhythmCommandResource>(setup.FromGhost(Combos.Span[i]));

			component.HeroModeAllowedCommands.SetLength(HeroModeAllowedCommands.GetLength());
			for (var i = 0; i < component.HeroModeAllowedCommands.GetLength(); i++)
				component.HeroModeAllowedCommands.Span[i] = new GameResource<RhythmCommandResource>(setup.FromGhost(HeroModeAllowedCommands.Span[i]));
		}
	}
}