using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using Microsoft.Extensions.Logging;
using StormiumTeam.GameBase.GamePlay.Health.Systems.Pass;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using ZLogger;

namespace StormiumTeam.GameBase.GamePlay.Health.Systems
{
	public class HealthSystem : GameAppSystem, IUpdateSimulationPass
	{
		private ILogger logger;
		
		public HealthSystem(WorldCollection collection) : base(collection)
		{
			AddDisposable(eventList = new PooledList<ModifyHealthEvent>());
			
			DependencyResolver.Add(() => ref logger);
		}

		private PooledList<ModifyHealthEvent> eventList;

		private EntityQuery livableQuery;
		private EntityQuery healthQuery;
		private EntityQuery eventQuery;

		public void OnSimulationUpdate()
		{
			eventList.Clear();

			var healthEventAccessor = GetAccessor<ModifyHealthEvent>();
			foreach (var entity in eventQuery ??= CreateEntityQuery(new[]
			{
				typeof(ModifyHealthEvent)
			}))
			{
				if (!GameWorld.Contains(healthEventAccessor[entity].Target.Handle))
				{
					logger.ZLogWarning("Entity 'Target' is invalid ({0})", healthEventAccessor[entity].Target);
					continue;
				}
				
				eventList.Add(healthEventAccessor[entity]);
			}
			eventQuery.RemoveAllEntities();

			foreach (var register in World.DefaultSystemCollection.Passes)
			{
				if (register is not RegisterHealthProcessPass healthProcessPass)
					continue;

				healthProcessPass.HealthSystem = this;
				healthProcessPass.Trigger();
			}
			
			var livableHealthAccessor = GetAccessor<LivableHealth>();
			foreach (var entity in livableQuery ??= CreateEntityQuery(new[]
			{
				typeof(LivableHealth)
			}))
			{
				livableHealthAccessor[entity] = default;
			}

			var ownerAccessor          = GetAccessor<Owner>();
			var concreteHealthAccessor = GetAccessor<ConcreteHealthValue>();
			foreach (var entity in healthQuery ??= CreateEntityQuery(new[]
			{
				typeof(HealthDescription),
				typeof(ConcreteHealthValue),
				typeof(Owner)
			}))
			{
				// This can happen if the net client didn't received the owner yet or if it switched owner and the health entities hasn't been synchronizd yet
				if (!livableQuery.MatchAgainst(ownerAccessor[entity].Target.Handle))
					continue;

				ref var          livableHealth  = ref livableHealthAccessor[ownerAccessor[entity].Target.Handle];
				ref readonly var concreteHealth = ref concreteHealthAccessor[entity];
				livableHealth.Value += concreteHealth.Value;
				livableHealth.Max   += concreteHealth.Max;
			}
		}

		public Span<ModifyHealthEvent> HealthEvents => eventList.Span;
	}
}