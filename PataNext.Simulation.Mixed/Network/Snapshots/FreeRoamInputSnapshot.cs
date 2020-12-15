using System.Runtime.CompilerServices;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Utility.InterTick;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.Module.Simulation.Network.Snapshots
{
	public unsafe struct FreeRoamInputSnapshot : IReadWriteSnapshotData<FreeRoamInputSnapshot>, ISnapshotSyncWithComponent<FreeRoamInputComponent>
	{
		public class Serializer : DeltaComponentSerializerBase<FreeRoamInputSnapshot, FreeRoamInputComponent>
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

		public uint                  HorizontalMovement;
		public InterFramePressAction Up, Down;

		public static readonly BoundedRange BoundedRange = new(-1, 1, 0.1f);

		public void Serialize(in BitBuffer buffer, in FreeRoamInputSnapshot baseline, in EmptySnapshotSetup setup)
		{
			fixed (FreeRoamInputSnapshot* t = &baseline)
			{
				if (Unsafe.AreSame(ref this, ref *t))
				{
					buffer.AddBool(false);
					return;
				}
			}

			buffer.AddBool(true)
			      .AddUIntD4Delta(HorizontalMovement, baseline.HorizontalMovement)
			      .AddIntDelta(Up.Pressed, baseline.Up.Pressed)
			      .AddIntDelta(Up.Released, baseline.Up.Released)
			      .AddIntDelta(Down.Pressed, baseline.Down.Pressed)
			      .AddIntDelta(Down.Released, baseline.Down.Released);
		}

		public void Deserialize(in BitBuffer buffer, in FreeRoamInputSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			HorizontalMovement = buffer.ReadUIntD4Delta(baseline.HorizontalMovement);
			Up.Pressed         = buffer.ReadIntDelta(baseline.Up.Pressed);
			Up.Released        = buffer.ReadIntDelta(baseline.Up.Released);
			Down.Pressed       = buffer.ReadIntDelta(baseline.Down.Pressed);
			Down.Released      = buffer.ReadIntDelta(baseline.Down.Released);
		}

		public void FromComponent(in FreeRoamInputComponent component, in EmptySnapshotSetup setup)
		{
			HorizontalMovement = BoundedRange.Quantize(component.HorizontalMovement);
			Up                 = component.Up;
			Down               = component.Down;
		}

		public void ToComponent(ref FreeRoamInputComponent component, in EmptySnapshotSetup setup)
		{
			component.HorizontalMovement = BoundedRange.Dequantize(HorizontalMovement);
			component.Up                 = Up;
			component.Down               = Down;
		}
	}
}