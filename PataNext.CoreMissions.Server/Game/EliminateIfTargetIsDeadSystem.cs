using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.CoreMissions.Server.Game
{
	public struct EliminateIfTargetIsDead : IComponentData
	{
		public GameEntity Value;

		public EliminateIfTargetIsDead(GameEntity value) => Value = value;
	}

	public class EliminateIfTargetIsDeadSystem : GameAppSystem
	{
		public EliminateIfTargetIsDeadSystem([NotNull] WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery query;
		private EntityQuery validHealthmateMask;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			query               = CreateEntityQuery(new[] { typeof(EliminateIfTargetIsDead), typeof(LivableHealth) });
			validHealthmateMask = CreateEntityQuery(new[] { typeof(LivableHealth) });
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			validHealthmateMask.CheckForNewArchetypes();

			var targetAccessor = GetAccessor<EliminateIfTargetIsDead>();
			foreach (var entity in query)
			{
				ref readonly var target = ref targetAccessor[entity].Value;
				if (!GameWorld.Exists(target) || HasComponent<LivableIsDead>(target))
				{
					AddComponent(CreateEntity(), new ModifyHealthEvent(ModifyHealthType.SetNone, 0, Safe(entity)));
					AddComponent(entity, new LivableIsDead());
				}
			}
		}
	}
}