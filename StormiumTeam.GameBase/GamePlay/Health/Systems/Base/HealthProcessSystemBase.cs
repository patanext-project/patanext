using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using StormiumTeam.GameBase.GamePlay.Health.Systems.Pass;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.GamePlay.Health.Systems.Base
{
	public abstract class HealthProcessSystemBase<T> : GameAppSystem, IHealthProcessPass
		where T : struct, IComponentData
	{
		protected HealthProcessSystemBase(WorldCollection collection) : base(collection)
		{
		}

		protected abstract ConcreteHealthValue OnExecute(GameEntityHandle healthEntity, in GameEntity owner, Span<ModifyHealthEvent> healthEvents, ref T data);

		private EntityQuery healthQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			healthQuery = CreateEntityQuery(new[] {typeof(HealthDescription), typeof(T), typeof(ConcreteHealthValue)});
		}

		void IHealthProcessPass.OnTrigger(Span<ModifyHealthEvent> events)
		{
			var ownerAccessor     = GetAccessor<Owner>();
			var componentAccessor = GetAccessor<T>();
			var concreteAccessor  = GetAccessor<ConcreteHealthValue>();
			foreach (var entity in healthQuery)
			{
				concreteAccessor[entity] = OnExecute(entity, ownerAccessor[entity].Target, events, ref componentAccessor[entity]);
			}
		}
	}
}