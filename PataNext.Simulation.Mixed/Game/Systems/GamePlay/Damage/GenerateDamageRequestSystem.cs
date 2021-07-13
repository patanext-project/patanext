using Collections.Pooled;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Features;
using GameHost.Core.Threading;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Revolution.NetCode.Rpc;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Network.NetCodeRpc;
using PataNext.Module.Simulation.Systems;
using RevolutionSnapshot.Core.Buffers;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Damage
{
	[UpdateAfter(typeof(HitBoxAgainstEnemiesSystem))]
	public class GenerateDamageRequestSystem : GameAppSystem, IGameEventPass
	{
		public struct SystemEvent : IComponentData {}

		private GetFeature<ClientFeature> clients;
		private GetFeature<ServerFeature> servers;

		public readonly IScheduler Pre;
		
		public GenerateDamageRequestSystem([NotNull] WorldCollection collection) : base(collection)
		{
			Pre = new Scheduler();
			
			DependencyResolver.Add(() => ref clients, new FeatureDependencyStrategy<ClientFeature>(collection, f => f is ClientFeature));
			DependencyResolver.Add(() => ref servers, new FeatureDependencyStrategy<ServerFeature>(collection, f => f is ServerFeature));
		}
		
		private EntityQuery toDestroy;
		private EntityQuery eventQuery;

		public void OnGameEventPass()
		{
			(toDestroy ??= CreateEntityQuery(new[] {typeof(SystemEvent)})).RemoveAllEntities();
			
			// don't run PreScheduler before destroying entities since DamageRequestRpc create events with the SystemEvent component!
			Pre.Run();
			
			var eventAccessor = GetAccessor<HitBoxEvent>();
			foreach (var entity in eventQuery ??= CreateEntityQuery(new[]
			{
				typeof(HitBoxEvent)
			}))
			{
				var ev = eventAccessor[entity];

				if (!TryGetComponentData(ev.HitBox, out DamageFrameData frameData))
					if (TryGetComponentData(ev.HitBox, out UnitPlayState playState))
						frameData = new DamageFrameData(playState);

				var dmgEv = CreateEntity();
				AddComponent(dmgEv, new TargetDamageEvent(ev.Instigator, ev.Victim, -frameData.Attack));
				AddComponent(dmgEv, frameData);
				AddComponent(dmgEv, new Position {Value = ev.ContactPosition});
				AddComponent(dmgEv, new SystemEvent());
				var statusEffectBuffer = GameWorld.AddBuffer<DamageFrameDataStatusEffect>(dmgEv);
				if (TryGetComponentBuffer<DamageFrameDataStatusEffect>(ev.HitBox, out var hitBoxStatusBuffer))
				{
					statusEffectBuffer.Clear();
					statusEffectBuffer.AddRange(hitBoxStatusBuffer.Span);
				}

				foreach (var (ent, feature) in clients)
				{
					if (!ent.TryGet(out NetCodeRpcBroadcaster rpcBroadcaster))
						continue;

					// Serialize now the data of the event, and queue it for next net send.
					// (if we don't serialize now, this event may be sent with some modifications that the server shouldn't know)
					rpcBroadcaster.Queue(new DamageRequestRpc
					{
						Instigator = ev.Instigator,
						Victim     = ev.Victim,
						Damage     = -frameData.Attack,

						Position  = ev.ContactPosition,
						FrameData = UnsafeUtility.SameData(frameData, default) ? null : frameData
					});
				}

				if (servers.Count != 0)
					AddComponent(dmgEv, new NetworkedEntity());
			}
		}
	}
}