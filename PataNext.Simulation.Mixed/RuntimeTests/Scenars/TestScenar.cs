using System;
using System.Numerics;
using System.Threading.Tasks;
using Box2D.NetStandard.Collision.Shapes;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Structures;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.GameModes;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;

namespace PataNext.Module.Simulation.Game.Scenar
{
	public class TestScenarProvider : ScenarProvider
	{
		public TestScenarProvider(WorldCollection collection) : base(collection)
		{
		}

		public override ResPath ScenarPath => new(ResPath.EType.ClientResource, "guerro", "test", "scenar");

		public override IScenar Provide()
		{
			return new TestScenar(World);
		}
	}

	public class TestScenar : ScenarScriptServer
	{
		private SimpleDestroyableStructureProvider destroyableProvider;
		private GameEntity                         endFlag;
		private TeamEndFlagProvider                endFlagProvider;

		private GameEntity enemyTeam;

		private GameResourceDb<GameGraphicResource> graphicDb;

		private ResPathGen           resPathGen;
		private PlayerTeamProvider   teamProvider;
		private PlayableUnitProvider unitProvider;
		private IManagedWorldTime    worldTime;

		public TestScenar(WorldCollection wc) : base(wc)
		{
			DependencyResolver.Add(() => ref teamProvider);
			DependencyResolver.Add(() => ref unitProvider);
			DependencyResolver.Add(() => ref destroyableProvider);
			DependencyResolver.Add(() => ref endFlagProvider);

			DependencyResolver.Add(() => ref resPathGen);
			DependencyResolver.Add(() => ref worldTime);

			DependencyResolver.Add(() => ref graphicDb);
		}

		protected override Task OnStart()
		{
			if (!GameWorld.TryGetSingleton<ProtagonistTeamTag>(out GameEntityHandle protagonistTeamHandle))
				throw new InvalidOperationException("No protagonist team found");

			enemyTeam = Safe(teamProvider.SpawnEntityWithArguments(new CreatePlayerTeam()));
			AddComponent(enemyTeam, new SimulationAuthority());
			GetBuffer<TeamEnemies>(enemyTeam)
				.Add(new TeamEnemies(Safe(protagonistTeamHandle)));
			GetBuffer<TeamEnemies>(protagonistTeamHandle)
				.Add(new TeamEnemies(enemyTeam));

			using (var colliderEntity = World.Mgr.CreateEntity())
			{
				colliderEntity.Set<Shape>(new PolygonShape(new Vector2(-1.15f, 2.5f), new Vector2(+1.15f, 2.5f), new Vector2(-2, 0), new Vector2(+2, 0)));

				var args = new SimpleDestroyableStructureProvider.Create
				{
					Position           = new Vector3(3, 0, 0),
					Health             = 100,
					Visual             = graphicDb.GetOrCreate(resPathGen.Create(new[] {"Models", "GameModes", "Structures", "CobblestoneBarricade", "Prefab"}, ResPath.EType.ClientResource)),
					ColliderDefinition = colliderEntity,
					Area               = new ContributeToTeamMovableArea(0, 2)
				};

				GameWorld.Link(destroyableProvider.SpawnEntityWithArguments(args), enemyTeam.Handle, true);
				args.Position.X += 2;
				GameWorld.Link(destroyableProvider.SpawnEntityWithArguments(args), enemyTeam.Handle, true);
				args.Position.X += 2;
				GameWorld.Link(destroyableProvider.SpawnEntityWithArguments(args), enemyTeam.Handle, true);

				args.Position.X += 3;
				for (var i = 0; i < 4; i++)
				{
					args.Health += 50 + i * i * 5;
					GameWorld.Link(destroyableProvider.SpawnEntityWithArguments(args), enemyTeam.Handle, true);
				}
			}

			foreach (var child in GameWorld.Boards.Entity.GetLinkedEntities(enemyTeam.Id))
				AddComponent(child, new Relative<TeamDescription>(enemyTeam));

			endFlag = Safe(endFlagProvider.SpawnEntityWithArguments(new CreateTeamEndFlag
			{
				Direction = UnitDirection.Right,
				Position  = 12.5f,
				Teams = new[]
				{
					Safe(protagonistTeamHandle)
				},
				GraphicResource = graphicDb.GetOrCreate(resPathGen.Create(new[] {"Models", "GameModes", "Structures", "HeadOnFlag", "Flag"}, ResPath.EType.ClientResource))
			}));

			return Task.CompletedTask;
		}

		protected override async Task OnLoop()
		{
			if (HasComponent<EndFlagHasBeenPassed>(endFlag))
			{
				Console.WriteLine("EndFlag passed!");
				await Task.Delay(1000);

				AddComponent(CreateEntity(), new GameModeRequestEndRound());
			}
		}

		protected override Task OnCleanup(bool reuse)
		{
			RemoveEntity(endFlag);
			RemoveEntity(enemyTeam);

			return Task.CompletedTask;
		}
	}
}