using System;
using System.Numerics;
using BepuPhysics;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	[UpdateAfter(typeof(UnitPhysicsSystem))]
	public class UnitCollisionSystem : GameAppSystem, IPostUpdateSimulationPass
	{
		private PhysicsSystem     physicsSystem;
		private IManagedWorldTime worldTime;
		
		public UnitCollisionSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref physicsSystem);
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery unitQuery;
		private EntityQuery colliderMask;

		public void OnAfterSimulationUpdate()
		{
			colliderMask ??= CreateEntityQuery(new[] {typeof(Position), typeof(PhysicsCollider), typeof(EnvironmentCollider)});
			colliderMask.CheckForNewArchetypes();
			
			var colliderAccessor = GetAccessor<PhysicsCollider>();
			var positionAccessor = GetAccessor<Position>();
			var teamRelativeAccessor = GetAccessor<Relative<TeamDescription>>();
			foreach (var unit in unitQuery ??= CreateEntityQuery(new[]
			{
				typeof(UnitControllerState),
				typeof(Position),
				typeof(PhysicsCollider),
				typeof(Relative<TeamDescription>)
				// is simulation owned comp
			}))
			{
				if (!TryGetComponentBuffer<TeamEnemies>(teamRelativeAccessor[unit].Handle, out var enemyBuffer))
					continue;

				var thisShape    = colliderAccessor[unit].Shape;
				
				ref var thisPosition = ref positionAccessor[unit].Value;

				Vector3 thisVelocity = default;
				/*if (HasComponent(entity, velocityComponentType))
				{
					thisVelocity = velocityAccessor[entity].Value;
					if (hitBox.VelocityUseDelta)
						thisVelocity *= dt;
				}*/

				var prev = GetComponentData<UnitControllerState>(unit).PreviousPosition;
				foreach (var teamEntity in enemyBuffer)
				{
					foreach (var enemy in GetBuffer<TeamEntityContainer>(teamEntity.Team.Handle).Reinterpret<GameEntity>())
					{
						if (!colliderMask.MatchAgainst(enemy.Handle))
							continue;

						if (!physicsSystem.Sweep(enemy.Handle, thisShape, new RigidPose(thisPosition), new BodyVelocity(default), out _))
							continue;
						
						thisPosition += prev - thisPosition;

						if (physicsSystem.Sweep(enemy.Handle, thisShape, new RigidPose(thisPosition), new BodyVelocity(default), out var hit))
							thisPosition -= hit.normal * (float)(worldTime.Delta.TotalSeconds);
					}
				}
			}
		}
	}
}