using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.CoreMissions.Server.Providers
{
	public class CobblestoneBarricadeProvider : BaseProvider<CobblestoneBarricadeProvider.Create>
	{
		public struct Create
		{
			public int     Health;
			public Vector2 Position;
		}

		private GameResourceDb<GameGraphicResource> graphicDb;
		private SimpleDestroyableStructureProvider  parent;

		private ResPathGen resPathGen;

		private Entity colliderSettings;

		public CobblestoneBarricadeProvider([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref graphicDb);
			DependencyResolver.Add(() => ref parent);

			DependencyResolver.Add(() => ref resPathGen);

			AddDisposable(colliderSettings = World.Mgr.CreateEntity());
			colliderSettings.Set<Shape>(new PolygonShape(new Vector2(-1.15f, 2.5f), new Vector2(+1.15f, 2.5f), new Vector2(-2, 0), new Vector2(+2, 0)));
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			parent.GetComponents(entityComponents);
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			parent.SetEntityData(entity, new()
			{
				Area               = new(0, 2),
				Visual             = graphicDb.GetOrCreate(resPathGen.Create(new[] { "Models", "GameModes", "Structures", "CobblestoneBarricade", "Prefab" }, ResPath.EType.ClientResource)),
				ColliderDefinition = colliderSettings,
				Health             = data.Health,
				Position           = new(data.Position, 0)
			});
		}
	}
}