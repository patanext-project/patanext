using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.CoreMissions.Server.Providers
{
	public class DestructibleProvider : BaseProvider<DestructibleProvider.Create>
	{
		public struct Create
		{
			public int                               Health;
			public Vector2                           Position;
			public GameResource<GameGraphicResource> Graphic;

			public Shape[] Shapes;

			public Create(GameResource<GameGraphicResource> graphic, int health, Vector2 position, params Shape[] shapes)
			{
				Graphic  = graphic;
				Health   = health;
				Position = position;
				Shapes   = shapes;
			}
		}

		private SimpleDestroyableStructureProvider parent;

		private Entity colliderSettings;

		public DestructibleProvider([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref parent);

			AddDisposable(colliderSettings = World.Mgr.CreateEntity());
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			parent.GetComponents(entityComponents);
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			colliderSettings.Set(data.Shapes);

			parent.SetEntityData(entity, new()
			{
				Area               = new(0, 2),
				Visual             = data.Graphic,
				ColliderDefinition = colliderSettings,
				Health             = data.Health,
				Position           = new(data.Position, 0)
			});
		}
	}
}