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
using StormiumTeam.GameBase.Physics;
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
		private IPhysicsSystem     physicsSystem;
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
				
				ref var thisPosition = ref positionAccessor[unit].Value;

				Vector3 thisVelocity = default;

				var prev = GetComponentData<UnitControllerState>(unit).PreviousPosition;
				foreach (var teamEntity in enemyBuffer)
				{
					foreach (var enemy in GetBuffer<TeamEntityContainer>(teamEntity.Team.Handle).Reinterpret<GameEntity>())
					{
						if (!colliderMask.MatchAgainst(enemy.Handle))
							continue;

						if (!physicsSystem.Distance(enemy.Handle, unit, 0, default, new EntityOverrides {Position = thisPosition, Velocity = thisPosition - prev}, out var result))
							continue;
						
						thisPosition -= result.Distance * result.Normal;

						/*if (physicsSystem.Sweep(enemy.Handle, thisShape, new RigidPose(thisPosition), new BodyVelocity(default), out hit))
							thisPosition -= hit.normal * (float)(worldTime.Delta.TotalSeconds);*/
					}
				}
			}
		}
	}
}