using System;
using System.Threading.Tasks;
using BepuUtilities;
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

		public BasicTestGameModeSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref resPathGen);
			DependencyResolver.Add(() => ref worldTime);

			DependencyResolver.Add(() => ref playableUnitProvider);
			DependencyResolver.Add(() => ref abilityCollectionSystem);

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
			});

			for (var army = 0; army < entities.Length; army++)
			{
				var unitArray = entities[army];
				foreach (var unit in unitArray)
				{
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
						
						abilityCollectionSystem.SpawnFor(resPathGen.Create(new [] {"ability", "yari", "def_atk"}, ResPath.EType.MasterServer), unit);
					}
				}
			}
		}

		private GameEntity[][] SpawnArmy(((bool isLeader, string archetype)[] args, float armyPos)[] array, float spawnPosition = 0)
		{
			var playerEntity = GameWorld.CreateEntity();
			GameWorld.AddComponent(playerEntity, new PlayerDescription());
			GameWorld.AddComponent(playerEntity, new GameRhythmInputComponent());
			GameWorld.AddComponent(playerEntity, new PlayerIsLocal());

			var unitTarget = GameWorld.CreateEntity();
			GameWorld.AddComponent(unitTarget, new UnitTargetDescription());
			GameWorld.AddComponent(unitTarget, new Position());
			GameWorld.AddComponent(unitTarget, new Relative<PlayerDescription>(playerEntity));

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
							MovementAttackSpeed = 3.1f,
							Weight              = 8.5f,
							AttackSpeed         = 2f,
						},
						Direction = UnitDirection.Right
					});

					GameWorld.AddComponent(unit, new UnitArchetype(localArchetypeDb.GetOrCreate(new UnitArchetypeResource(array[army].args[u].archetype))));

					GameWorld.AddComponent(unit, new Relative<PlayerDescription>(playerEntity));
					GameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(unitTarget));
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
						GameWorld.AddComponent(playerEntity, new ServerCameraState
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