using System;
using System.Collections.Generic;
using System.Numerics;
using BepuUtilities;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Resources.Keys;
using PataNext.Module.Simulation.Systems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.GameModes
{
	public struct ReachScore : IComponentData
	{
		public float Value;
	}

	public class YaridaTrainingGameMode : GameAppSystem
	{
		private IManagedWorldTime worldTime;

		private PlayableUnitProvider    playableUnitProvider;
		private AbilityCollectionSystem abilityCollectionSystem;

		public YaridaTrainingGameMode(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);

			DependencyResolver.Add(() => ref playableUnitProvider);
			DependencyResolver.Add(() => ref abilityCollectionSystem);
		}

		GameResourceDb<UnitKitResource, UnitKitResourceKey>               localKitDb;
		GameResourceDb<UnitArchetypeResource, UnitArchetypeResourceKey>   localArchetypeDb;
		GameResourceDb<UnitAttachmentResource, UnitAttachmentResourceKey> localAttachDb;
		GameResourceDb<EquipmentResource, EquipmentResourceKey>           localEquipDb;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			localKitDb       = new GameResourceDb<UnitKitResource, UnitKitResourceKey>(GameWorld);
			localArchetypeDb = new GameResourceDb<UnitArchetypeResource, UnitArchetypeResourceKey>(GameWorld);
			localAttachDb    = new GameResourceDb<UnitAttachmentResource, UnitAttachmentResourceKey>(GameWorld);
			localEquipDb     = new GameResourceDb<EquipmentResource, EquipmentResourceKey>(GameWorld);
		}

		private GameEntity   GameMode;
		private GameEntity   RhythmEngine;
		private GameEntity   UberHero;
		private GameEntity[] Yarida;

		// will be used to check if the player want to forfeit
		private GameEntity RetreatAbility;

		public override bool CanUpdate()
		{
			return GameWorld.Contains(GameMode) && base.CanUpdate();
		}
		
		private TimeSpan startTime;
		protected override void OnUpdate()
		{
			base.OnUpdate();

			ref var gmData           = ref GetComponentData<YaridaTrainingGameModeData>(GameMode);
			ref var uberHeroPosition = ref GetComponentData<Position>(UberHero).Value;

			gmData.CurrUberHeroPos = uberHeroPosition.X;
			
			switch (gmData.Phase)
			{
				case YaridaTrainingGameModeData.EPhase.Waiting:
				{
					if (uberHeroPosition.X >= 0)
					{
						startTime = worldTime.Total;
						
						gmData.Phase               = YaridaTrainingGameModeData.EPhase.March;
						gmData.YaridaOvertakeCount = -1;
						foreach (var unit in Yarida)
						{
							GetComponentData<UnitStatistics>(unit).MovementAttackSpeed = 1f;
							GetComponentData<ReachScore>(unit).Value                   = 1;
						}
					}
					else
					{
						foreach (var unit in Yarida)
						{
							GetComponentData<UnitStatistics>(unit).MovementAttackSpeed = 0;
							GetComponentData<Position>(unit).Value                     = new Vector3(10, 0 ,0);
						}
					}

					break;
				}
				case YaridaTrainingGameModeData.EPhase.March:
				{
					// forfeit if true
					if (GetComponentData<AbilityState>(RetreatAbility).IsActive)
					{
						GetComponentData<RhythmEngineController>(RhythmEngine).StartTime = worldTime.Total.Add(TimeSpan.FromSeconds(2));
						
						gmData.Phase = YaridaTrainingGameModeData.EPhase.Waiting;
						
						uberHeroPosition.X                                                                                     = -10;
						GetComponentData<Position>(GetComponentData<Relative<UnitTargetDescription>>(UberHero).Target).Value.X = -10;
					}
					
					for (var i = 0; i < Yarida.Length; i++)
					{
						var unit      = Yarida[i];
						var position  = GetComponentData<Position>(unit).Value;
						var target    = GetComponentData<Relative<UnitTargetDescription>>(unit);
						var targetPos = GetComponentData<Position>(target.Target).Value;
						if (uberHeroPosition.X >= targetPos.X - 0.33f)
						{
							gmData.YaridaOvertakeCount = Math.Max(gmData.YaridaOvertakeCount, i);
							gmData.LastCheckpointTime  = (float) (worldTime.Total - startTime).TotalSeconds;
							gmData.LastCheckpointScore  = GetComponentData<ReachScore>(unit).Value;
						}
						else if (gmData.YaridaOvertakeCount < i && uberHeroPosition.X > position.X + 0.1f)
						{
							var checkpointPos = 20 + gmData.YaridaOvertakeCount * 10;

							uberHeroPosition.X                                                                                     = checkpointPos;
							GetComponentData<Position>(GetComponentData<Relative<UnitTargetDescription>>(UberHero).Target).Value.X = checkpointPos;
						}

						ref var reachScore = ref GetComponentData<ReachScore>(unit);
						if (Math.Abs(position.X - targetPos.X) < 0.2f)
						{
							reachScore.Value = Math.Max(0, reachScore.Value - 0.1f * (float) worldTime.Delta.TotalSeconds);
						}
					}

					if (gmData.YaridaOvertakeCount + 1 >= Yarida.Length)
					{
						GetComponentData<UnitDirection>(UberHero) = UnitDirection.Left;
						gmData.Phase                              = YaridaTrainingGameModeData.EPhase.Backward;
					}

					break;
				}
				case YaridaTrainingGameModeData.EPhase.Backward:
					if (uberHeroPosition.X <= 0)
					{
						GetComponentData<UnitDirection>(UberHero) = UnitDirection.Right;
						gmData.Phase                              = YaridaTrainingGameModeData.EPhase.Waiting;
						
						uberHeroPosition.X                                                                                     = -10;
						GetComponentData<Position>(GetComponentData<Relative<UnitTargetDescription>>(UberHero).Target).Value.X = -10;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Start(int yaridaCount)
		{
			GameMode = GameWorld.CreateEntity();
			GameWorld.AddComponent(GameMode, new YaridaTrainingGameModeData());
			
			UberHero = SpawnUberHero(-10);
			
			Yarida = new GameEntity[yaridaCount];
			for (var i = 0; i != yaridaCount; i++)
			{
				Yarida[i] = SpawnYarida(10, 10 + i * 10);
			}
		}

		private GameEntity SpawnUberHero(float positionX)
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

			GameEntity first = default;
			for (var i = 0; i < 7; i++)
			{
				var unit = playableUnitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
				{
					Statistics = new UnitStatistics
					{
						BaseWalkSpeed       = 2,
						FeverWalkSpeed      = 2.2f,
						MovementAttackSpeed = 3.1f,
						Weight              = 8.5f,
					},
					Direction = UnitDirection.Right
				});
				GameWorld.GetComponentData<Position>(unit).Value.X       = positionX;
				GameWorld.GetComponentData<Position>(unitTarget).Value.X = positionX;

				GameWorld.AddComponent(unit, new Relative<PlayerDescription>(playerEntity));
				GameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(unitTarget));
				GameWorld.AddComponent(unit, new UnitEnemySeekingState());

				var displayedEquip = GameWorld.AddBuffer<UnitDisplayedEquipment>(unit);
				if (first == default)
				{
					GameWorld.AddComponent(unit, new UnitTargetControlTag());
					GameWorld.AddComponent(unit, new UnitTargetOffset());
					GameWorld.AddComponent(unit, new UnitArchetype(localArchetypeDb.GetOrCreate(new UnitArchetypeResourceKey("st:pn/archetype/uberhero_std_unit"))));
					GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResourceKey("taterazay"))));

					GameWorld.AddComponent(playerEntity, new ServerCameraState
					{
						Data =
						{
							Mode   = CameraMode.Forced,
							Offset = RigidTransform.Identity,
							Target = unit
						}
					});

					displayedEquip.Add(new UnitDisplayedEquipment
					{
						Attachment = localAttachDb.GetOrCreate("st:pn/equip_root/mask"),
						Resource   = localEquipDb.GetOrCreate("Masks/n_kibadda")
					});
					displayedEquip.Add(new UnitDisplayedEquipment
					{
						Attachment = localAttachDb.GetOrCreate("st:pn/equip_root/l_eq"),
						Resource   = localEquipDb.GetOrCreate("Shields/default_shield")
					});
					displayedEquip.Add(new UnitDisplayedEquipment
					{
						Attachment = localAttachDb.GetOrCreate("st:pn/equip_root/r_eq"),
						Resource   = localEquipDb.GetOrCreate("Swords/default_sword")
					});

					first = unit;
				}
				else
				{
					GameWorld.AddComponent(unit, new UnitTargetOffset {Value = 1 + i * 0.5f});
					GameWorld.AddComponent(unit, new UnitArchetype(localArchetypeDb.GetOrCreate(new UnitArchetypeResourceKey("st:pn/archetype/patapon_std_unit"))));
					GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResourceKey("yarida"))));

					displayedEquip.Add(new UnitDisplayedEquipment
					{
						Attachment = localAttachDb.GetOrCreate("st:pn/equip_root/r_eq"),
						Resource   = localEquipDb.GetOrCreate("Spears/default_spear_smaller")
					});
					displayedEquip.Add(new UnitDisplayedEquipment
					{
						Attachment = localAttachDb.GetOrCreate("st:pn/equip_root/helmet"),
						Resource   = localEquipDb.GetOrCreate("Helmets/default_helmet_small")
					});
				}

				abilityCollectionSystem.SpawnFor("march", unit);
				abilityCollectionSystem.SpawnFor("backward", unit);
				RetreatAbility = abilityCollectionSystem.SpawnFor("retreat", unit);
				abilityCollectionSystem.SpawnFor("jump", unit);
				abilityCollectionSystem.SpawnFor("party", unit, jsonData: new {disableEnergy = first != unit});
				abilityCollectionSystem.SpawnFor("charge", unit);
				abilityCollectionSystem.SpawnFor("CTate.BasicDefendFrontal", unit);
				abilityCollectionSystem.SpawnFor("CTate.BasicDefendStay", unit, AbilitySelection.Top);
				abilityCollectionSystem.SpawnFor("CTate.EnergyField", unit);

				GameWorld.AddComponent(unit, new Relative<RhythmEngineDescription>(rhythmEngine));
			}

			RhythmEngine = rhythmEngine;

			return first;
		}

		private GameEntity SpawnYarida(float initialPosX, float positionX)
		{
			var unit = playableUnitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
			{
				Statistics = new UnitStatistics
				{
					BaseWalkSpeed       = 0f,
					FeverWalkSpeed      = 0f,
					MovementAttackSpeed = 0f,
					Weight              = 8.5f,
				},
				Direction = UnitDirection.Right
			});

			var tt = GameWorld.CreateEntity();
			GameWorld.AddComponent(tt, new UnitTargetDescription());
			GameWorld.AddComponent(tt, new Position());
			GameWorld.GetComponentData<Position>(tt).Value.X = positionX + initialPosX;
			GameWorld.GetComponentData<Position>(unit).Value.X = initialPosX;

			GameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResourceKey("yarida"))));
			GameWorld.AddComponent(unit, new UnitEnemySeekingState());
			GameWorld.AddComponent(unit, new UnitTargetOffset());
			GameWorld.AddComponent(unit, new ReachScore {Value = 1});
			GameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(tt));

			var displayedEquip = GameWorld.AddBuffer<UnitDisplayedEquipment>(unit);
			displayedEquip.Add(new UnitDisplayedEquipment
			{
				Attachment = localAttachDb.GetOrCreate("Mask"),
				Resource   = localEquipDb.GetOrCreate("Masks/n_yarida")
			});
			displayedEquip.Add(new UnitDisplayedEquipment
			{
				Attachment = localAttachDb.GetOrCreate("RightEquipment"),
				Resource   = localEquipDb.GetOrCreate("Spears/default_spear")
			});

			return unit;
		}
	}
}