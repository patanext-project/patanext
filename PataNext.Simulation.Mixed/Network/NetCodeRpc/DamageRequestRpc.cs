using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.NetCode.Rpc;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Time.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Network.NetCodeRpc
{
	public struct DamageRequestRpc
	{
		public GameEntity Instigator;
		public GameEntity Victim;
		public double     Damage;

		public Vector3?         Position;
		public DamageFrameData? FrameData;

		public class Serializer : NetCodeRpcSerializerBase<DamageRequestRpc, GhostSetup>
		{
			[StructLayout(LayoutKind.Explicit)]
			private struct Union
			{
				[FieldOffset(0)] public long   Long;
				[FieldOffset(0)] public double Double;
			}

			private GenerateDamageRequestSystem generateDamageRequestSystem;

			public Serializer([NotNull] Context context) : base(context)
			{
				DependencyResolver.Add(() => ref generateDamageRequestSystem);
			}

			private BoundedRange positionBound = new BoundedRange(-5000, 5000, 0.01f);
			private BoundedRange statBound     = new BoundedRange(-1000, 1000, 0.001f);

			protected override void OnSerialize(in BitBuffer bitBuffer, in DamageRequestRpc data, GhostSetup setup)
			{
				bitBuffer.AddGhostDelta(setup.ToGhost(data.Instigator), default);
				bitBuffer.AddGhostDelta(setup.ToGhost(data.Victim), default);
				bitBuffer.AddLong(new Union {Double = data.Damage}.Long);

				// ---------------------------------
				// POSITION
				if (data.Position is { } position)
				{
					bitBuffer.AddBool(true);

					bitBuffer.AddUInt(positionBound.Quantize(position.X));
					bitBuffer.AddUInt(positionBound.Quantize(position.Y));
					bitBuffer.AddUInt(positionBound.Quantize(position.Z));
				}
				else
					bitBuffer.AddBool(false);

				// ---------------------------------
				// FRAME DATA
				if (data.FrameData is { } frameData)
				{
					bitBuffer.AddBool(true);

					bitBuffer.AddInt(frameData.Attack);

					bitBuffer.AddUInt(statBound.Quantize(frameData.Weight));
					bitBuffer.AddUInt(statBound.Quantize(frameData.KnockBackPower));

					bitBuffer.AddUInt(statBound.Quantize(frameData.IgnoreDefense));
					bitBuffer.AddUInt(statBound.Quantize(frameData.IgnoreReceiveDamage));
				}
				else
					bitBuffer.AddBool(false);

				Console.WriteLine($"Sending dmg event! {bitBuffer.Length}B");
			}

			protected override void OnDeserialize(in BitBuffer bitBuffer, GhostSetup setup)
			{
				Console.WriteLine($"Receiving dmg event! {bitBuffer.Length}B - {(global::System.Threading.Thread.CurrentThread.Name)}");

				DamageRequestRpc data;
				data.Instigator = setup.FromGhost(bitBuffer.ReadGhostDelta(default));
				data.Victim     = setup.FromGhost(bitBuffer.ReadGhostDelta(default));
				data.Damage     = new Union {Long = bitBuffer.ReadLong()}.Double;

				TargetDamageEvent damageEvent;
				damageEvent.Instigator = data.Instigator;
				damageEvent.Victim     = data.Victim;
				damageEvent.Damage     = data.Damage;
				
				// ---------------------------------
				// POSITION
				Position? forSchedulingPosition;
				if (bitBuffer.ReadBool())
				{
					forSchedulingPosition = new Position(
						positionBound.Dequantize(bitBuffer.ReadUInt()),
						positionBound.Dequantize(bitBuffer.ReadUInt()),
						positionBound.Dequantize(bitBuffer.ReadUInt())
					);
				}
				else
					forSchedulingPosition = null;
				
				// ---------------------------------
				// FRAME DATA
				DamageFrameData? forSchedulingFrameData;
				if (bitBuffer.ReadBool())
				{
					DamageFrameData frameData;

					frameData.Attack = bitBuffer.ReadInt();

					frameData.Weight         = statBound.Dequantize(bitBuffer.ReadUInt());
					frameData.KnockBackPower = statBound.Dequantize(bitBuffer.ReadUInt());

					frameData.IgnoreDefense       = statBound.Dequantize(bitBuffer.ReadUInt());
					frameData.IgnoreReceiveDamage = statBound.Dequantize(bitBuffer.ReadUInt());

					forSchedulingFrameData = frameData;
				}
				else
					forSchedulingFrameData = null;
				
				
				generateDamageRequestSystem.Pre.Schedule(t =>
				{
					var handle = GameWorld.CreateEntity();
					GameWorld.AddComponent(handle, t.damageEvent);
					GameWorld.AddComponent(handle, t.instigator);

					if (t.position is {} position)
						GameWorld.AddComponent(handle, position);
					if (t.forSchedulingFrameData is { } frameData)
						GameWorld.AddComponent(handle, frameData);
					
					GameWorld.AddComponent(handle, new NetworkedEntity());
					GameWorld.AddComponent(handle, new GenerateDamageRequestSystem.SystemEvent());
				}, (damageEvent, instigator: new SnapshotEntity.ForcedInstigatorId(Instigator.InstigatorId), position: forSchedulingPosition, forSchedulingFrameData), default);
			}
		}
	}
}