using System;
using System.Numerics;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using Box2D.NetStandard.Collision.Shapes;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Health.Systems;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.GameModes
{
	public struct ReachScore : IComponentData
	{
		public float Value;
	}

	public class BasicTestGameModeSystem : MissionGameModeBase<BasicTestGameMode>
	{
		private ResPathGen        resPathGen;
		private IManagedWorldTime worldTime;

		private PlayableUnitProvider    playableUnitProvider;
		private AbilityCollectionSystem abilityCollectionSystem;

		private IPhysicsSystem physicsSystem;

		private DefaultHealthProvider healthProvider;

		public BasicTestGameModeSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref resPathGen);
			DependencyResolver.Add(() => ref worldTime);

			DependencyResolver.Add(() => ref playableUnitProvider);
			DependencyResolver.Add(() => ref abilityCollectionSystem);
			DependencyResolver.Add(() => ref physicsSystem);

			DependencyResolver.Add(() => ref healthProvider);

			DependencyResolver.Add(() => ref localKitDb);
			DependencyResolver.Add(() => ref localArchetypeDb);
			DependencyResolver.Add(() => ref localAttachDb);
			DependencyResolver.Add(() => ref localEquipDb);
		}

		GameResourceDb<UnitKitResource>        localKitDb;
		GameResourceDb<UnitArchetypeResource>  localArchetypeDb;
		GameResourceDb<UnitAttachmentResource> localAttachDb;
		GameResourceDb<EquipmentResource>      localEquipDb;

		protected override async Task GameModeInitialisation()
		{
			// We need to wait some frames before ability providers are registered
			// (I'm thinking of a better way, perhaps a method in AbilityCollectionSystem called SpawnForLater(CancellationToken))
			// (Or perhaps we should wait for all modules to be loaded, and their AppObjects to have no dependencies left?)
			var frameToWait = 16;
			while (frameToWait-- > 0)
			{
				await Task.Yield();
			}

			var playerEntity = GameWorld.CreateEntity();
			GameWorld.AddComponent(playerEntity, new PlayerDescription());
			GameWorld.AddComponent(playerEntity, new GameRhythmInputComponent());
			GameWorld.AddComponent(playerEntity, new PlayerIsLocal());
			GameWorld.AddComponent(playerEntity, new InputAuthority());
			GameWorld.AddBuffer<OwnedRelative<UnitDescription>>(playerEntity);

			var rhythmEngine = GameWorld.CreateEntity();
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineDescription());
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineController {State      = RhythmEngineState.Playing, StartTime = worldTime.Total.Add(TimeSpan.FromSeconds(2))});
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineSettings {BeatInterval = TimeSpan.FromSeconds(0.5), MaxBeat   = 4});
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineLocalState());
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineExecutingCommand());
			GameWorld.AddComponent(rhythmEngine, new Relative<PlayerDescription>(Safe(playerEntity)));
			GameWorld.AddComponent(rhythmEngine, GameWorld.AsComponentType<RhythmEngineCommandProgressBuffer>());
			GameWorld.AddComponent(rhythmEngine, GameWorld.AsComponentType<RhythmEnginePredictedCommandBuffer>());
			GameWorld.AddComponent(rhythmEngine, new GameCommandState());
			GameWorld.AddComponent(rhythmEngine, new SimulationAuthority());
			GameCombo.AddToEntity(GameWorld, rhythmEngine);
			RhythmSummonEnergy.AddToEntity(GameWorld, rhythmEngine);

			var entities = SpawnArmy(new[]
			{
				// Hatapon
				(new[]
				{
					(true, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer))
				}, 0f),
				// UberHero
				(new[]
				{
					(false, resPathGen.Create(new[] {"archetype", "uberhero_std_unit"}, ResPath.EType.MasterServer))
				}, 3f),
				/*(new[]
				{
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer))
				}, 3f),
				(new[]
				{
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer)),
					(false, resPathGen.Create(new[] {"archetype", "patapon_std_unit"}, ResPath.EType.MasterServer))
				}, -3f)*/
			}, playerEntity, rhythmEngine, out var thisTarget, -10);
			AddComponent(thisTarget, new Relative<PlayerDescription>(Safe(playerEntity)));

			var enemies = SpawnArmy(new[]
			{
				(new[]
				{
					(true, resPathGen.Create(new[] {"archetype", "uberhero_std_unit"}, ResPath.EType.MasterServer))
				}, 0f)
			}, default, default, out var enemyTarget, 5);

			var team = CreateEntity();
			AddComponent(team, new TeamDescription());
			AddComponent(team, new TeamMovableArea());
			AddComponent(thisTarget, new Relative<TeamDescription>(Safe(team)));
			GameWorld.AddBuffer<TeamAllies>(team);
			GameWorld.AddBuffer<TeamEntityContainer>(team);

			var enemyTeam = CreateEntity();
			AddComponent(team, new TeamDescription());
			AddComponent(enemyTeam, new TeamDescription());
			AddComponent(enemyTeam, new TeamMovableArea());
			GameWorld.AddBuffer<TeamAllies>(enemyTeam);
			GameWorld.AddBuffer<TeamEntityContainer>(enemyTeam);

			var simpleCollidable = enemies[0][0];
			AddComponent(simpleCollidable, new Relative<TeamDescription>(Safe(enemyTeam)));
			AddComponent(simpleCollidable, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource("taterazay"))));

			GetComponentData<UnitStatistics>(simpleCollidable).Attack = 28;

			var ability = abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "tate", "def_atk"}, ResPath.EType.MasterServer), simpleCollidable);
			GetComponentData<AbilityState>(ability).Phase                 = EAbilityPhase.Active;
			GetComponentData<OwnerActiveAbility>(simpleCollidable).Active = Safe(ability);

			GameWorld.AddBuffer<UnitWeakPoint>(simpleCollidable)
			         .Add(new UnitWeakPoint(new Vector3(0, 1, 0)));
			GameWorld.GetComponentData<UnitDirection>(simpleCollidable) = UnitDirection.Left;

			var unitColliderSettings = World.Mgr.CreateEntity();
			unitColliderSettings.Set<Shape>(new PolygonShape(0.5f, 0.75f, new Vector2(0, 0.75f), 0));

			physicsSystem.AssignCollider(simpleCollidable, unitColliderSettings);

			GameWorld.AddBuffer<TeamEnemies>(team).Add(new TeamEnemies(Safe(enemyTeam)));
			GameWorld.AddBuffer<TeamEnemies>(enemyTeam).Add(new TeamEnemies(Safe(team)));
			GameWorld.AddComponent(team, new SimulationAuthority());
			GameWorld.AddComponent(enemyTeam, new SimulationAuthority());

			var tower = GameWorld.CreateEntity();
			GameWorld.AddComponent(tower, new Position(x: -10));
			GameWorld.AddComponent(tower, new EnvironmentCollider());
			GameWorld.AddComponent(tower, new Relative<TeamDescription>(Safe(enemyTeam)));

			var towerColliderSettings = World.Mgr.CreateEntity();
			towerColliderSettings.Set<Shape>(new PolygonShape(new Vector2(-3, 0), new Vector2(3, 0), new Vector2(-2, 3), new Vector2(2, 3)));
			physicsSystem.AssignCollider(tower, towerColliderSettings);

			for (var army = 0; army < entities.Length; army++)
			{
				var unitArray = entities[army];
				foreach (var unit in unitArray)
				{
					physicsSystem.AssignCollider(unit, unitColliderSettings);

					AddComponent(unit, new SimulationAuthority());
					AddComponent(unit, new Relative<TeamDescription>(Safe(team)));

					abilityCollectionSystem.SpawnFor("march", unit);
					abilityCollectionSystem.SpawnFor("backward", unit);
					abilityCollectionSystem.SpawnFor("retreat", unit);
					abilityCollectionSystem.SpawnFor("jump", unit);
					abilityCollectionSystem.SpawnFor("party", unit, jsonData: new {disableEnergy = army != 0});
					abilityCollectionSystem.SpawnFor("charge", unit);

					var displayedEquip = GetBuffer<UnitDisplayedEquipment>(unit);
					if (army == 1)
					{
						void setTaterazay()
						{
							GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource("taterazay"))));

							GetComponentData<UnitStatistics>(unit).Attack = 28;

							displayedEquip.Add(new UnitDisplayedEquipment
							{
								Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "mask"}, ResPath.EType.MasterServer)),
								Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "masks", "taterazay"}, ResPath.EType.ClientResource))
							});
							displayedEquip.Add(new UnitDisplayedEquipment
							{
								Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "l_eq"}, ResPath.EType.MasterServer)),
								Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "shields", "default_shield"}, ResPath.EType.ClientResource))
							});
							displayedEquip.Add(new UnitDisplayedEquipment
							{
								Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "r_eq"}, ResPath.EType.MasterServer)),
								Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "swords", "default_sword"}, ResPath.EType.ClientResource))
							});

							abilityCollectionSystem.SpawnFor("CTate.BasicDefendFrontal", unit);
							abilityCollectionSystem.SpawnFor("CTate.BasicDefendStay", unit, AbilitySelection.Bottom);
							abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "tate", "counter"}, ResPath.EType.MasterServer), unit, AbilitySelection.Top);
							abilityCollectionSystem.SpawnFor("CTate.EnergyField", unit);

							abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "tate", "def_atk"}, ResPath.EType.MasterServer), unit);
						}

						void setGuardira()
						{
							GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource("guardira"))));

							GetComponentData<UnitStatistics>(unit).Attack              = 28;
							GetComponentData<UnitStatistics>(unit).MovementAttackSpeed = 1.9f;
							GetComponentData<UnitStatistics>(unit).Weight              = 15;

							displayedEquip.Add(new UnitDisplayedEquipment
							{
								Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "mask"}, ResPath.EType.MasterServer)),
								Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "masks", "guardira"}, ResPath.EType.ClientResource))
							});
							displayedEquip.Add(new UnitDisplayedEquipment
							{
								Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "r_eq"}, ResPath.EType.MasterServer)),
								Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "greatshields", "default_greatshield"}, ResPath.EType.ClientResource))
							});

							abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "guard", "def_def"}, ResPath.EType.MasterServer), unit);
						}

						void setWooyari()
						{
							GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource("wooyari"))));

							GetComponentData<UnitStatistics>(unit).Attack              = 28;
							GetComponentData<UnitStatistics>(unit).MovementAttackSpeed = 2.1f;
							GetComponentData<UnitStatistics>(unit).Weight              = 15;

							displayedEquip.Add(new UnitDisplayedEquipment
							{
								Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "mask"}, ResPath.EType.MasterServer)),
								Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "masks", "yarida"}, ResPath.EType.ClientResource))
							});
							displayedEquip.Add(new UnitDisplayedEquipment
							{
								Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "r_eq"}, ResPath.EType.MasterServer)),
								Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "pikes", "default_pike"}, ResPath.EType.ClientResource))
							});

							abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "pike", "multi_atk"}, ResPath.EType.MasterServer), unit);
						}

						setTaterazay();
					}
					else if (army == 2)
					{
						GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource("yarida"))));

						GetComponentData<UnitStatistics>(unit).Attack = 10;

						displayedEquip.Add(new UnitDisplayedEquipment
						{
							Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "r_eq"}, ResPath.EType.MasterServer)),
							Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "spears", "default_spear:small"}, ResPath.EType.ClientResource))
						});
						displayedEquip.Add(new UnitDisplayedEquipment
						{
							Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "helmet"}, ResPath.EType.MasterServer)),
							Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "helmets", "default_helmet:small"}, ResPath.EType.ClientResource))
						});

						abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "yari", "def_atk"}, ResPath.EType.MasterServer), unit);
					}
					else if (army == 3)
					{
						GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource("yumiyacha"))));

						GetComponentData<UnitStatistics>(unit).AttackSeekRange = 28f;
						GetComponentData<UnitStatistics>(unit).Attack          = 8;

						displayedEquip.Add(new UnitDisplayedEquipment
						{
							Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "r_eq"}, ResPath.EType.MasterServer)),
							Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "bows", "default_bow:small"}, ResPath.EType.ClientResource))
						});
						displayedEquip.Add(new UnitDisplayedEquipment
						{
							Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "helmet"}, ResPath.EType.MasterServer)),
							Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "helmets", "default_helmet:small"}, ResPath.EType.ClientResource))
						});

						abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "yumi", "def_atk"}, ResPath.EType.MasterServer), unit);
						abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "yumi", "snipe_atk"}, ResPath.EType.MasterServer), unit, AbilitySelection.Top);
					}
				}
			}
		}

		private GameEntityHandle[][] SpawnArmy(((bool isLeader, string archetype)[] args, float armyPos)[] array,
		                                 in GameEntityHandle                                         player, in GameEntityHandle rhythmEngine, out GameEntityHandle unitTarget,
		                                 float                                                       spawnPosition = 0)
		{
			unitTarget = GameWorld.CreateEntity();
			GameWorld.AddComponent(unitTarget, new UnitTargetDescription());
			GameWorld.AddComponent(unitTarget, new Position());
			GameWorld.AddComponent(unitTarget, new UnitEnemySeekingState());

			GameWorld.GetComponentData<Position>(unitTarget).Value.X = spawnPosition;

			var result = new GameEntityHandle[array.Length][];
			for (var army = 0; army < result.Length; army++)
			{
				var entities = result[army] = new GameEntityHandle[array[army].args.Length];
				for (var u = 0; u < entities.Length; u++)
				{
					var unit = playableUnitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
					{
						Statistics = new UnitStatistics
						{
							BaseWalkSpeed       = 2,
							FeverWalkSpeed      = 2.2f,
							MovementAttackSpeed = 2.2f,
							Weight              = 8.5f,
							AttackSpeed         = 2f,
							AttackSeekRange     = 16f,
						},
						Direction = UnitDirection.Right
					});

					if (player != default)
					{
						GameWorld.Link(unit, player, true);
						GameWorld.AddComponent(unit, new Owner(Safe(player)));
					}

					GameWorld.AddComponent(unit, new UnitArchetype(localArchetypeDb.GetOrCreate(new UnitArchetypeResource(array[army].args[u].archetype))));

					GameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(Safe(unitTarget)));

					if (player != default)
						GameWorld.AddComponent(unit, new Relative<PlayerDescription>(Safe(player)));
					if (rhythmEngine != default)
						GameWorld.AddComponent(unit, new Relative<RhythmEngineDescription>(Safe(rhythmEngine)));

					GameWorld.AddComponent(unit, new UnitEnemySeekingState());
					GameWorld.AddComponent(unit, new UnitTargetOffset
					{
						Idle   = array[army].armyPos + UnitTargetOffset.CenterComputeV1(u, array[army].args.Length, 0.5f),
						Attack = UnitTargetOffset.CenterComputeV1(u, array[army].args.Length, 0.5f)
					});

					GameWorld.AddBuffer<UnitDisplayedEquipment>(unit);

					healthProvider.SpawnEntityWithArguments(new DefaultHealthProvider.Create
					{
						value = 300,
						max   = 300,
						owner = Safe(unit)
					});

					if (array[army].args[u].isLeader)
					{
						GameWorld.AddComponent(unit, new UnitTargetControlTag());
						GameWorld.AddComponent(unit, new UnitTargetOffset());

						if (player != default)
							GameWorld.AddComponent(player, new ServerCameraState
							{
								Data =
								{
									Mode   = CameraMode.Forced,
									Offset = RigidTransform.Identity,
									Target = Safe(unit)
								}
							});
					}

					result[army][u] = unit;
				}
			}

			return result;
		}
	}
}