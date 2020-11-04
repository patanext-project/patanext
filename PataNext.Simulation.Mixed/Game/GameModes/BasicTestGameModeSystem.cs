using System;
using System.Numerics;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
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

		private PhysicsSystem physicsSystem;

		public BasicTestGameModeSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref resPathGen);
			DependencyResolver.Add(() => ref worldTime);

			DependencyResolver.Add(() => ref playableUnitProvider);
			DependencyResolver.Add(() => ref abilityCollectionSystem);
			DependencyResolver.Add(() => ref physicsSystem);

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

			var rhythmEngine = GameWorld.CreateEntity();
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineDescription());
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineController {State      = RhythmEngineState.Playing, StartTime = worldTime.Total.Add(TimeSpan.FromSeconds(2))});
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineSettings {BeatInterval = TimeSpan.FromSeconds(0.5), MaxBeat   = 4});
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineLocalState());
			GameWorld.AddComponent(rhythmEngine, new RhythmEngineExecutingCommand());
			GameWorld.AddComponent(rhythmEngine, new Relative<PlayerDescription>(playerEntity));
			GameWorld.AddComponent(rhythmEngine, GameWorld.AsComponentType<RhythmEngineLocalCommandBuffer>());
			GameWorld.AddComponent(rhythmEngine, GameWorld.AsComponentType<RhythmEnginePredictedCommandBuffer>());
			GameWorld.AddComponent(rhythmEngine, new GameCommandState());
			GameWorld.AddComponent(rhythmEngine, new IsSimulationOwned());
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
				}, 6f),
				(new[]
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
				}, -3f)
			}, playerEntity, rhythmEngine, out var thisTarget, -10);
			AddComponent(thisTarget, new Relative<PlayerDescription>(playerEntity));

			var enemies = SpawnArmy(new[]
			{
				(new[]
				{
					(true, resPathGen.Create(new[] {"archetype", "uberhero_std_unit"}, ResPath.EType.MasterServer))
				}, 0f)
			}, default, rhythmEngine, out var enemyTarget);

			var team = CreateEntity();
			AddComponent(team, new TeamDescription());
			GameWorld.AddBuffer<TeamAllies>(team);
			GameWorld.AddBuffer<TeamEntityContainer>(team);
			var enemyBuffer = GameWorld.AddBuffer<TeamEnemies>(team);

			var enemyTeam = CreateEntity();
			AddComponent(enemyTeam, new TeamDescription());
			GameWorld.AddBuffer<TeamAllies>(enemyTeam);
			GameWorld.AddBuffer<TeamEntityContainer>(enemyTeam);

			var simpleCollidable = enemies[0][0];
			AddComponent(simpleCollidable, new Relative<TeamDescription>(enemyTeam));

			physicsSystem.BufferPool.Take(1, out Buffer<CompoundChild> b);
			b[0] = new CompoundChild
			{
				ShapeIndex = physicsSystem.Simulation.Shapes.Add(new Box(1, 1.5f, 1)),
				LocalPose  = new RigidPose(new Vector3(0, 0.75f, 0))
			};

			physicsSystem.SetColliderShape(simpleCollidable, new Compound(b));

			enemyBuffer.Add(new TeamEnemies(enemyTeam));

			for (var army = 0; army < entities.Length; army++)
			{
				var unitArray = entities[army];
				foreach (var unit in unitArray)
				{
					AddComponent(unit, new Relative<TeamDescription>(team));

					abilityCollectionSystem.SpawnFor("march", unit);
					abilityCollectionSystem.SpawnFor("backward", unit);
					abilityCollectionSystem.SpawnFor("retreat", unit);
					abilityCollectionSystem.SpawnFor("jump", unit);
					abilityCollectionSystem.SpawnFor("party", unit, jsonData: new {disableEnergy = army != 0});
					abilityCollectionSystem.SpawnFor("charge", unit);

					var displayedEquip = GetBuffer<UnitDisplayedEquipment>(unit);
					if (army == 1)
					{
						// it's kinda a special case
						GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource("taterazay"))));

						displayedEquip.Add(new UnitDisplayedEquipment
						{
							Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "mask"}, ResPath.EType.MasterServer)),
							Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "masks", "kibadda"}, ResPath.EType.ClientResource))
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
						abilityCollectionSystem.SpawnFor("CTate.BasicDefendStay", unit, AbilitySelection.Top);
						abilityCollectionSystem.SpawnFor("CTate.EnergyField", unit);
						
						abilityCollectionSystem.SpawnFor(resPathGen.Create(new[] {"ability", "tate", "def_atk"}, ResPath.EType.MasterServer), unit);
					}
					else
					{
						GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource("yarida"))));

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
				}
			}
		}

		private GameEntity[][] SpawnArmy(((bool isLeader, string archetype)[] args, float armyPos)[] array, in GameEntity player, in GameEntity rhythmEngine, out GameEntity unitTarget, float spawnPosition = 0)
		{
			unitTarget = GameWorld.CreateEntity();
			GameWorld.AddComponent(unitTarget, new UnitTargetDescription());
			GameWorld.AddComponent(unitTarget, new Position());

			GameWorld.GetComponentData<Position>(unitTarget).Value.X = spawnPosition;

			var result = new GameEntity[array.Length][];
			for (var army = 0; army < result.Length; army++)
			{
				var entities = result[army] = new GameEntity[array[army].args.Length];
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
						},
						Direction = UnitDirection.Right
					});

					GameWorld.AddComponent(unit, new UnitArchetype(localArchetypeDb.GetOrCreate(new UnitArchetypeResource(array[army].args[u].archetype))));

					GameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(unitTarget));

					if (player != default)
						GameWorld.AddComponent(unit, new Relative<PlayerDescription>(player));
					if (rhythmEngine != default)
						GameWorld.AddComponent(unit, new Relative<RhythmEngineDescription>(rhythmEngine));

					GameWorld.AddComponent(unit, new UnitEnemySeekingState());
					GameWorld.AddComponent(unit, new UnitTargetOffset
					{
						Idle   = array[army].armyPos + UnitTargetOffset.CenterComputeV1(u, array[army].args.Length, 0.5f),
						Attack = UnitTargetOffset.CenterComputeV1(u, array[army].args.Length, 0.5f)
					});

					Console.WriteLine($"{army} -> {array[army].armyPos};{u} -> {array[army].armyPos + u * 0.5f}");

					GameWorld.AddBuffer<UnitDisplayedEquipment>(unit);

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
									Target = unit
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