using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay;
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
		private DestructibleProvider                parent;

		private ResPathGen resPathGen;

		public CobblestoneBarricadeProvider([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref graphicDb);
			DependencyResolver.Add(() => ref parent);

			DependencyResolver.Add(() => ref resPathGen);
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			parent.GetComponents(entityComponents);
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			var args = new DestructibleProvider.Create(
				graphicDb.GetOrCreate(resPathGen.Create(new[] { "Models", "GameModes", "Structures", "CobblestoneBarricade", "Prefab" }, ResPath.EType.ClientResource)),
				data.Health,
				data.Position,
				new PolygonShape(new Vector2(-1.15f, 2.5f), new Vector2(+1.15f, 2.5f), new Vector2(-2, 0), new Vector2(+2, 0))
			);
			parent.SetEntityData(entity, args);
		}
	}
}