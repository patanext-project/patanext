using System;
using System.Numerics;
using BepuPhysics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Time.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace StormiumTeam.GameBase.GamePlay.HitBoxes
{
	public class HitBoxAgainstEnemiesSystem : GameAppSystem, IUpdateSimulationPass
	{
		private struct SystemEvent : IComponentData
		{
		}

		private IPhysicsSystem     physicsSystem;
		private IManagedWorldTime worldTime;

		public HitBoxAgainstEnemiesSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref physicsSystem);
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery eventQuery;

		private EntityQuery hitboxQuery;
		private EntityQuery colliderMask;

		public void OnSimulationUpdate()
		{
			(eventQuery ??= CreateEntityQuery(new [] {typeof(SystemEvent)})).RemoveAllEntities();
			
			var dt = (float) worldTime.Delta.TotalSeconds;

			colliderMask ??= CreateEntityQuery(new[] {typeof(PhysicsCollider), typeof(Position)});
			colliderMask.CheckForNewArchetypes();

			var velocityComponentType = AsComponentType<Velocity>();

			var hitBoxAccessor               = GetAccessor<HitBox>();
			var hitBoxAgainstEnemiesAccessor = GetAccessor<HitBoxAgainstEnemies>();
			var positionAccessor             = GetAccessor<Position>();
			var velocityAccessor             = GetAccessor<Velocity>();
			foreach (var entity in hitboxQuery ??= CreateEntityQuery(new[]
			{
				typeof(HitBox),
				typeof(HitBoxAgainstEnemies),
				typeof(Position),
				typeof(PhysicsCollider)
			}))
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

						if (historyBuffer.Reinterpret<GameEntity>().Contains(enemy.Value))
							continue;
						
						if (!physicsSystem.Distance(enemy.Value.Handle, entity, 0, default, new EntityOverrides {Position = thisPosition, Velocity = thisVelocity}, out var result))
							continue;

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

						if (historyBuffer.IsCreated)
							historyBuffer.Add(new HitBoxHistory(enemy.Value));
					}
				}
			}
		}
	}
}