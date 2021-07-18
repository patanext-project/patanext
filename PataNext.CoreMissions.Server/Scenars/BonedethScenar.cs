using System;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.CoreMissions.Mixed.Missions;
using PataNext.CoreMissions.Server.Game;
using PataNext.CoreMissions.Server.Providers;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Components.GamePlay.Special.Squad;
using PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreMissions.Server.Scenars
{
	public class BonedethScenar : MissionScenarScript
	{
		private TowerBastionProvider bastionProvider;

		private SimpleAiBotUnitProvider botUnitProvider;

		private CobblestoneBarricadeProvider cobblestoneBarricadeProvider;

		public BonedethScenar(WorldCollection wc) : base(wc)
		{
			DependencyResolver.Add(() => ref cobblestoneBarricadeProvider);
			DependencyResolver.Add(() => ref botUnitProvider);
			DependencyResolver.Add(() => ref bastionProvider);
		}

		protected override Task OnStart()
		{
			base.OnStart();

			{
				var barricade = cobblestoneBarricadeProvider.SpawnEntityWithArguments(new()
				{
					Health   = 1500,
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
							Health = 300,

							Attack      = 12,
							AttackSpeed = 4,

							AttackSeekRange = 20
						},

						HealthBegin = 300,
						Collider    = new(1, 1.5f),

						InitialPosition = new(15, 2.5f), // spawn on top of barricade

						Visual = GraphicDb.GetOrCreate(ResPathGen.Create(new[] { "Models", "Patapon", "PataponYarida" }, ResPath.EType.ClientResource)),

						Equipments = new()
						{
							{ GetAttachment(new[] { "equip_root", "helm" }), GetEquipment(new[] { "equipment", "helm", "default_helm" }) },
							{ GetAttachment(new[] { "equip_root", "r_eq" }), GetEquipment(new[] { "equipment", "spear", "default_spear" }) }
						}
					},

					Actions = new[]
					{
						(TimeSpan.FromSeconds(2), new Func<GameEntityHandle, GameEntityHandle>(handle => CreateAbility(handle, new[] { "ability", "yarida", "default_attack" })))
					}
				});
				AddComponent(unit, new RemoveGravityUntilDead());
				AddComponent(unit, new EliminateIfTargetIsDead(Safe(barricade)));

				GameWorld.Link(unit, EnemyTeam.Handle, true);

				// Create a bastion that will spawn Tate enemies
				// linked to the stone barricade
				// And another with Yari enemies
				{
					var arguments = new BastionDynamicGroupProvider.Create();
					arguments.SetProviderForAll(botUnitProvider, new SimpleAiBotUnitProvider.Create
					{
						Parent = new()
						{
							Direction = UnitDirection.Left,
							Statistics = new()
							{
								Health              = 500,
								BaseWalkSpeed       = 2,
								MovementAttackSpeed = 2,

								Weight = 8,

								AttackSeekRange = 20,
								AttackSpeed     = 4,

								Attack  = 8,
								Defense = 10
							},

							HealthBegin = 500,
							Collider    = new(1, 1.5f),

							Visual = GraphicDb.GetOrCreate(ResPathGen.Create(new[] { "Models", "Patapon", "PataponTaterazay" }, ResPath.EType.ClientResource)),

							Equipments = new()
							{
								{ GetAttachment(new[] { "equip_root", "helm" }), GetEquipment(new[] { "equipment", "helm", "default_helm" }) },
								{ GetAttachment(new[] { "equip_root", "l_eq" }), GetEquipment(new[] { "equipment", "shield", "default_shield" }) },
								{ GetAttachment(new[] { "equip_root", "r_eq" }), GetEquipment(new[] { "equipment", "sword", "default_sword" }) }
							}
						},

						Actions = new[]
						{
							(TimeSpan.FromSeconds(2), new Func<GameEntityHandle, GameEntityHandle>(handle => CreateAbility(handle, new[] { "ability", "taterazay", "default_attack" })))
						}
					}, 3);

					var bastion = bastionProvider.SpawnEntityWithArguments(arguments);
					GameWorld.Link(bastion, EnemyTeam.Handle, true);

					AddComponent(bastion, new AutoSquadUnitDisplacement { Space = 0.5f });
					AddComponent(bastion, new BastionSpawnAllIfAllDead { Delay  = TimeSpan.FromSeconds(8) });

					AddComponent(bastion, new EliminateIfTargetIsDead(Safe(barricade)));

					GetComponentData<Position>(bastion).Value = GetComponentData<Position>(barricade).Value;

					// Yari

					arguments = new();
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
								AttackSpeed     = 4,

								Attack  = 8,
								Defense = 0
							},

							HealthBegin = 200,
							Collider    = new(1, 1.5f),

							Visual = GraphicDb.GetOrCreate(ResPathGen.Create(new[] { "Models", "Patapon", "PataponYarida" }, ResPath.EType.ClientResource)),

							Equipments = new()
							{
								{ GetAttachment(new[] { "equip_root", "helm" }), GetEquipment(new[] { "equipment", "helm", "default_helm" }) },
								{ GetAttachment(new[] { "equip_root", "r_eq" }), GetEquipment(new[] { "equipment", "spear", "default_spear" }) }
							}
						},

						Actions = new[]
						{
							(TimeSpan.FromSeconds(2), new Func<GameEntityHandle, GameEntityHandle>(handle => CreateAbility(handle, new[] { "ability", "yarida", "default_attack" })))
						}
					}, 3);

					bastion = bastionProvider.SpawnEntityWithArguments(arguments);
					GameWorld.Link(bastion, EnemyTeam.Handle, true);

					AddComponent(bastion, new AutoSquadUnitDisplacement { Space = 0.5f });
					AddComponent(bastion, new BastionSpawnAllIfAllDead { Delay  = TimeSpan.FromSeconds(10) });

					AddComponent(bastion, new EliminateIfTargetIsDead(Safe(barricade)));

					GetComponentData<Position>(bastion).Value = GetComponentData<Position>(barricade).Value;
				}
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
	}
}