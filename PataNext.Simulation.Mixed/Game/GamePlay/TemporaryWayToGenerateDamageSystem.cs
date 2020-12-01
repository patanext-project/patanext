using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Time.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay
{
	[UpdateAfter(typeof(HitBoxAgainstEnemiesSystem))]
	public class TemporaryWayToGenerateDamageSystem : GameAppSystem, IUpdateSimulationPass
	{
		private struct SystemEvent : IComponentData {}
		
		private IManagedWorldTime worldTime;
		
		public TemporaryWayToGenerateDamageSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery toDestroy;
		private EntityQuery eventQuery;

		public void OnSimulationUpdate()
		{
			(toDestroy ??= CreateEntityQuery(new[] {typeof(SystemEvent)})).RemoveAllEntities();
			
			var eventAccessor = GetAccessor<HitBoxEvent>();
			foreach (var entity in eventQuery ??= CreateEntityQuery(new[]
			{
				typeof(HitBoxEvent)
			}))
			{
				var ev = eventAccessor[entity];

				var atk    = GetComponentData<UnitPlayState>(ev.HitBox.Handle).Attack;
				var health = GetComponentData<LivableHealth>(ev.Victim.Handle);
				AddComponent(CreateEntity(), new ModifyHealthEvent(ModifyHealthType.Add, -atk, ev.Victim));

				if (health.Value - atk <= 0)
				{
					GetComponentData<Position>(ev.Victim.Handle).Value.X += 5;
					AddComponent(CreateEntity(), new ModifyHealthEvent(ModifyHealthType.SetMax, 0, ev.Victim));
				}

				var t = CreateEntity();
				AddComponent(t, new TargetDamageEvent(ev.Instigator, ev.Victim, -atk));
				AddComponent(t, new Position {Value = ev.ContactPosition});
				AddComponent(t, new SystemEvent());
			}
		}
	}
}