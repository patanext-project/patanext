using System;
using System.Collections.Generic;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using NetFabric.Hyperlinq;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;
using StormiumTeam.GameBase.Utility.Misc.EntitySystem;

namespace StormiumTeam.GameBase.GamePlay.HitBoxes
{
	public class HitBoxAgainstEnemiesSystem : GameAppSystem, IUpdateSimulationPass
	{
		private struct SystemEvent : IComponentData
		{
		}

		private IPhysicsSystem    physicsSystem;
		private IManagedWorldTime worldTime;
		private IBatchRunner      batchRunner;

		private readonly IScheduler structuralScheduler;

		public HitBoxAgainstEnemiesSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref physicsSystem);
			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref batchRunner);

			structuralScheduler = new Scheduler();
		}

		private EntityQuery eventQuery;

		private EntityQuery hitboxQuery;
		private EntityQuery colliderMask;

		private ArchetypeSystem<WorldTime> foreachSystem;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			eventQuery   = CreateEntityQuery(new[] {typeof(SystemEvent)});
			colliderMask = CreateEntityQuery(new[] {typeof(PhysicsCollider), typeof(Position)}, new[] {typeof(LivableIsDead)});

			hitboxQuery = CreateEntityQuery(new[]
			{
				typeof(HitBox),
				typeof(HitBoxAgainstEnemies),
				typeof(Position),
				typeof(PhysicsCollider)
			});

			foreachSystem = new ArchetypeSystem<WorldTime>(OnForeachHitBoxes, hitboxQuery);
		}

		private void OnForeachHitBoxes(in ReadOnlySpan<GameEntityHandle> entities, in SystemState<WorldTime> systemState)
		{
			var dt = (float) systemState.Data.Delta.TotalSeconds;

			var velocityComponentType = AsComponentType<Velocity>();

			var hitBoxAccessor               = GetAccessor<HitBox>();
			var hitBoxAgainstEnemiesAccessor = GetAccessor<HitBoxAgainstEnemies>();
			var positionAccessor             = GetAccessor<Position>();
			var velocityAccessor             = GetAccessor<Velocity>();

			foreach (ref readonly var entity in entities)
			{
				if (!TryGetComponentBuffer<TeamEnemies>(hitBoxAgainstEnemiesAccessor[entity].EnemyBufferSource.Handle, out var enemyBuffer))
					continue;

				ref var hitBox = ref hitBoxAccessor[entity];
				if (TryGetComponentBuffer<HitBoxHistory>(entity, out var historyBuffer)
				    && hitBox.MaxHits > 0 && historyBuffer.Count >= hitBox.MaxHits)
					continue;

				var thisPosition = positionAccessor[entity].Value;

				Vector3 thisVelocity = default;
				if (HasComponent(entity, velocityComponentType))
				{
					thisVelocity = velocityAccessor[entity].Value;
					if (hitBox.VelocityUseDelta)
						thisVelocity *= dt;
				}

				foreach (var teamEnemy in enemyBuffer.Span)
				{
					var entityContainer = GetBuffer<TeamEntityContainer>(teamEnemy.Team.Handle);
					foreach (var enemy in entityContainer.Span)
					{
						if (!colliderMask.MatchAgainst(enemy.Value.Handle))
							continue;
						
						if (historyBuffer.Contains((ref HitBoxHistory history) => ref history.Victim, enemy.Value))
							continue;

						if (!physicsSystem.Distance(enemy.Value.Handle, entity, 0, default, new EntityOverrides {Position = thisPosition, Velocity = thisVelocity}, out var result))
							continue;

						structuralScheduler.Schedule(args =>
						{
							var (entity, enemy, result) = args;

							var ev = CreateEntity();
							TryGetComponentData(entity, out Owner instigator);
							AddComponent(ev, new HitBoxEvent
							{
								HitBox     = Safe(entity),
								Instigator = instigator.Target,
								Victim     = enemy.Value,

								ContactPosition = result.Position,
								ContactNormal   = result.Normal
							});

							AddComponent(ev, new SystemEvent());
						}, (entity, enemy, result), default);

						if (historyBuffer.IsCreated)
							historyBuffer.Add(new HitBoxHistory(enemy.Value, result.Position, result.Normal));
					}
				}
			}
		}

		public void OnSimulationUpdate()
		{
			colliderMask.CheckForNewArchetypes();

			foreachSystem.PrepareData(worldTime.ToStruct());

			var request = batchRunner.Queue(foreachSystem);
			{
				// Operations that can execute while the parallel system is active
				eventQuery.RemoveAllEntities();
			}
			batchRunner.WaitForCompletion(request);

			structuralScheduler.Run();
		}
	}
}