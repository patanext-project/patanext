using System;
using System.Numerics;
using BepuPhysics;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Time.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace StormiumTeam.GameBase.GamePlay.HitBoxes
{
	public class HitBoxAgainstEnemiesSystem : GameAppSystem, IUpdateSimulationPass
	{
		private PhysicsSystem     physicsSystem;
		private IManagedWorldTime worldTime;

		public HitBoxAgainstEnemiesSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref physicsSystem);
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery hitboxQuery;
		private EntityQuery colliderMask;

		public void OnSimulationUpdate()
		{
			var dt = (float) worldTime.Delta.TotalSeconds;

			colliderMask ??= CreateEntityQuery(new[] {typeof(PhysicsCollider), typeof(Position)});
			colliderMask.CheckForNewArchetypes();

			var velocityComponentType = AsComponentType<Velocity>();

			var hitBoxAccessor               = GetAccessor<HitBox>();
			var hitBoxAgainstEnemiesAccessor = GetAccessor<HitBoxAgainstEnemies>();
			var positionAccessor             = GetAccessor<Position>();
			var velocityAccessor             = GetAccessor<Velocity>();
			var colliderAccessor             = GetAccessor<PhysicsCollider>();
			foreach (var entity in hitboxQuery ??= CreateEntityQuery(new[]
			{
				typeof(HitBox),
				typeof(HitBoxAgainstEnemies),
				typeof(Position),
				typeof(PhysicsCollider)
			}))
			{
				if (!TryGetComponentBuffer<TeamEnemies>(hitBoxAgainstEnemiesAccessor[entity].EnemyBufferSource, out var enemyBuffer))
					continue;

				ref var hitBox = ref hitBoxAccessor[entity];
				if (TryGetComponentBuffer<HitBoxHistory>(entity, out var historyBuffer)
				    && hitBox.MaxHits > 0 && historyBuffer.Count >= hitBox.MaxHits)
					continue;

				var thisShape    = colliderAccessor[entity].Shape;
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
					var entityContainer = GetBuffer<TeamEntityContainer>(teamEnemy.Team);
					foreach (var enemy in entityContainer.Span)
					{
						if (!colliderMask.MatchAgainst(enemy.Value))
							continue;

						if (!physicsSystem.Sweep(enemy.Value, thisShape, new RigidPose(thisPosition), new BodyVelocity(thisVelocity), out var hit))
							continue;
						
						var ev = GameWorld.CreateEntity();
						GameWorld.AddComponent(ev, new Position());
						GameWorld.AddComponent(ev, new RemoveEntityEndTime());

						if (historyBuffer.IsCreated)
							historyBuffer.Add(new HitBoxHistory(ev, enemy.Value));
					}
				}
			}
		}
	}
}