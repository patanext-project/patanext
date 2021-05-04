using System;
using System.Numerics;
using System.Threading.Tasks;
using Box2D.NetStandard.Collision.Shapes;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Structures;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Game.Visuals;
using PataNext.Module.Simulation.GameModes;
using PataNext.Module.Simulation.Network.Snapshots;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.GamePlay.Health.Systems;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

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
					Health             = 300,
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
					args.Health += 100 + i * i * 5;
					GameWorld.Link(destroyableProvider.SpawnEntityWithArguments(args), enemyTeam.Handle, true);
				}
			}

			/*using (var colliderEntity = World.Mgr.CreateEntity())
			{
				var target = Focus(Safe(CreateEntity()));
				target.AddData(new UnitTargetDescription())
				      .AddData(UnitDirection.Left)
				      .AddData(new UnitEnemySeekingState())
				      .AddData(new Position(x: 5));

				GameWorld.Link(target.Handle, enemyTeam.Handle, true);

				var unitFocus = Focus(Safe(unitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
				{
					Direction = UnitDirection.Left,
					Statistics = new UnitStatistics()
					{
						Health              = 100,
						Attack              = 10,
						AttackSpeed         = 2,
						BaseWalkSpeed       = 1,
						FeverWalkSpeed      = 1,
						MovementAttackSpeed = 1f,
						AttackSeekRange     = 20,
						Weight              = 10
					}
				})));

				unitFocus.AddData(new UnitBodyCollider(1, 1.5f))
				         .AddData(new Relative<UnitTargetDescription>(target.Entity))
				         .AddData<SimulationAuthority>()
				         .AddData<MovableAreaAuthority>();

				unitFocus.AddData(new EntityVisual(graphicDb.GetOrCreate(resPathGen.Create(new []
				{
					"Models",
					"Patapon",
					"PataponYarida"
				}, ResPath.EType.ClientResource)), true));

				if (World.TryGet(out AbilityCollectionSystem abilities))
				{
					var ab = abilities.SpawnFor(resPathGen.Create(new[] {"ability", "yarida", "default_attack"}, ResPath.EType.MasterServer), unitFocus.Handle);

					AddComponent(ab, new SimulationAuthority());
					GetComponentData<AbilityState>(ab).Phase = EAbilityPhase.Active;

					unitFocus.GetData<OwnerActiveAbility>().Active = Safe(ab);
				}

				if (World.TryGet(out DefaultHealthProvider healthProvider))
				{
					healthProvider.SpawnEntityWithArguments(new()
					{
						value = unitFocus.GetData<UnitStatistics>().Health,
						max   = unitFocus.GetData<UnitStatistics>().Health,
						owner = unitFocus.Entity
					});

					unitFocus.GetData<LivableHealth>() = new()
					{
						Value = unitFocus.GetData<UnitStatistics>().Health,
						Max   = unitFocus.GetData<UnitStatistics>().Health
					};
				}

				Console.WriteLine($"{target.Entity}; {unitFocus.Entity}");

				GameWorld.Link(unitFocus.Handle, enemyTeam.Handle, true);
			}*/

			foreach (var child in GameWorld.Boards.Entity.GetLinkedEntities(enemyTeam.Id))
				AddComponent(child, new Relative<TeamDescription>(enemyTeam));

			endFlag = Safe(endFlagProvider.SpawnEntityWithArguments(new CreateTeamEndFlag
			{
				Direction = UnitDirection.Right,
				Position  = 40f,
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