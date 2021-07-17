using System;
using System.Numerics;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.CoreMissions.Mixed;
using PataNext.CoreMissions.Mixed.Missions;
using PataNext.CoreMissions.Server.Game;
using PataNext.CoreMissions.Server.Providers;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Special.Squad;
using PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Game.Scenar;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreMissions.Server.Scenars
{
	public class BonedethScenar : MissionScenarScript
	{
		public class Provider : ScenarProvider
		{
			public Provider(WorldCollection collection) : base(collection)
			{
			}

			public override ResPath ScenarPath => BonedethMissionRegister.ScenarPath;

			public override IScenar Provide()
			{
				return new BonedethScenar(World);
			}
		}

		protected override Task OnStart()
		{
			base.OnStart();

			{
				var barricade = cobblestoneBarricadeProvider.SpawnEntityWithArguments(new()
				{
					Health   = 800,
					Position = new(15, 0)
				});
				GameWorld.Link(barricade, EnemyTeam.Handle, true);

				var unit = botUnitProvider.SpawnEntityWithArguments(new()
				{
					Parent = new()
					{
						Direction = UnitDirection.Left,
						Statistics = new()
						{
							Health = 100,
							
							Attack = 8,
							AttackSpeed = 2,
							
							AttackSeekRange = 20
						},

						HealthBegin = 100,
						Collider    = new(1, 1.5f),

						InitialPosition = new Vector2(15, 2.5f), // spawn on top of barricade

						Visual = GraphicDb.GetOrCreate(ResPathGen.Create(new[] { "Models", "Patapon", "PataponYarida" }, ResPath.EType.ClientResource))
					},
					
					Actions = new[]
					{
						(TimeSpan.FromSeconds(2), new Func<GameEntityHandle, GameEntityHandle>(handle =>
						{
							var ab = abilityCollection.SpawnFor(ResPathGen.Create(new[] { "ability", "yarida", "default_attack" }, ResPath.EType.MasterServer), handle);
							AddComponent(ab, new SimulationAuthority());

							return ab;
						})),
						(TimeSpan.FromSeconds(2), null)
					}
				});
				AddComponent(unit, new RemoveGravityUntilDead());
				AddComponent(unit, new EliminateIfTargetIsDead(Safe(barricade)));

				GameWorld.Link(unit, EnemyTeam.Handle, true);
			}

			{
				var barricade = cobblestoneBarricadeProvider.SpawnEntityWithArguments(new()
				{
					Health   = 800,
					Position = new(8, 0)
				});
				GameWorld.Link(barricade, EnemyTeam.Handle, true);

				var arguments = new BastionDynamicGroupProvider.Create();
				arguments.SetProviderForAll(botUnitProvider, new SimpleAiBotUnitProvider.Create
				{
					Parent = new()
					{
						Direction = UnitDirection.Left,
						Statistics = new()
						{
							Health              = 200,
							BaseWalkSpeed       = 2,
							MovementAttackSpeed = 2,

							Weight = 8,
							
							AttackSeekRange = 20,
							AttackSpeed = 2,
							
							Attack = 5,
							Defense = 2
						},

						HealthBegin = 200,
						Collider    = new(1, 1.5f),

						Visual = GraphicDb.GetOrCreate(ResPathGen.Create(new[] { "Models", "Patapon", "PataponTaterazay" }, ResPath.EType.ClientResource))
					},

					Actions = new[]
					{
						(TimeSpan.FromSeconds(2), new Func<GameEntityHandle, GameEntityHandle>(handle =>
						{
							var ab = abilityCollection.SpawnFor(ResPathGen.Create(new[] { "ability", "taterazay", "default_attack" }, ResPath.EType.MasterServer), handle);
							AddComponent(ab, new SimulationAuthority());

							return ab;
						})),
						(TimeSpan.FromSeconds(2), null)
					}
				}, 4);

				var bastion = bastionProvider.SpawnEntityWithArguments(arguments);
				GameWorld.Link(bastion, EnemyTeam.Handle, true);

				AddComponent(bastion, new AutoSquadUnitDisplacement { Space = 0.5f });
				AddComponent(bastion, new BastionSpawnAllIfAllDead { Delay  = TimeSpan.FromSeconds(5) });

				AddComponent(bastion, new EliminateIfTargetIsDead(Safe(barricade)));

				GetComponentData<Position>(bastion).Value.X = 8;
			}

			foreach (var entity in GameWorld.Boards.Entity.GetLinkedEntities(EnemyTeam.Id))
				AddComponent(entity, new Relative<TeamDescription>(EnemyTeam));

			return Task.CompletedTask;
		}

		protected override async Task OnLoop()
		{
		}

		protected override async Task OnCleanup(bool reuse)
		{
		}

		private CobblestoneBarricadeProvider cobblestoneBarricadeProvider;

		private SimpleAiBotUnitProvider botUnitProvider;
		private TowerBastionProvider    bastionProvider;

		public BonedethScenar(WorldCollection wc) : base(wc)
		{
			DependencyResolver.Add(() => ref cobblestoneBarricadeProvider);
			DependencyResolver.Add(() => ref botUnitProvider);
			DependencyResolver.Add(() => ref bastionProvider);
		}
	}
}