using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Systems;

namespace PataNext.Module.Simulation.Game.Providers
{
	public class FreeRoamUnitProvider : PlayableUnitProvider
	{
		private IPhysicsSystem physicsSystem;

		public FreeRoamUnitProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref physicsSystem);
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);

			entityComponents.AddRange(new[]
			{
				GameWorld.AsComponentType<UnitFreeRoamMovement>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			base.SetEntityData(entity, data);

			var unitColliderSettings = World.Mgr.CreateEntity();
			unitColliderSettings.Set<Shape>(new PolygonShape(0.5f, 0.75f, new Vector2(0, 0.75f), 0));
			physicsSystem.AssignCollider(entity, unitColliderSettings);
		}
	}
}