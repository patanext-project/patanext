using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.Components;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Utility;
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
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.GameModes.InBasement
{
	public class AtCityGameModeSystem : GameModeSystemBase<AtCityGameModeData>
	{
		public struct PlayerFreeRoamCharacter : IComponentData
		{
			public GameEntity Entity;
		}

		private FreeRoamUnitProvider unitProvider;
		private NetReportTimeSystem  reportTimeSystem;
		
		private AbilityCollectionSystem abilityCollectionSystem;

		private IManagedWorldTime worldTime;
		
		private ResPathGen resPathGen;
		
		GameResourceDb<UnitKitResource>       localKitDb;
		GameResourceDb<UnitArchetypeResource> localArchetypeDb;
		GameResourceDb<UnitAttachmentResource> localAttachDb;
		GameResourceDb<EquipmentResource> localEquipDb;
		
		public AtCityGameModeSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref resPathGen);
			
			DependencyResolver.Add(() => ref unitProvider);
			DependencyResolver.Add(() => ref reportTimeSystem);
			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref abilityCollectionSystem);
			
			DependencyResolver.Add(() => ref localKitDb);
			DependencyResolver.Add(() => ref localArchetypeDb);
			DependencyResolver.Add(() => ref localAttachDb);
			DependencyResolver.Add(() => ref localEquipDb);
		}

		private EntityQuery playerQuery, playerWithoutInputQuery, playerWithInputQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			playerQuery             = CreateEntityQuery(new[] {typeof(PlayerDescription)});
			playerWithInputQuery    = QueryWith(playerQuery, new[] {typeof(FreeRoamInputComponent)});
			playerWithoutInputQuery = QueryWithout(playerQuery, new[] {typeof(FreeRoamInputComponent)});
		}

		private void CreateCharacterAt(float xPos, GameEntity character, GameEntity team, UnitDirection direction)
		{
			var player = GetComponentData<Relative<PlayerDescription>>(character.Handle).Handle;

			// Add RhythmEngine (fully simulated by client)
			var rhythmEngine = GameWorld.CreateEntity();
			AddComponent(rhythmEngine, new RhythmEngineDescription());
			AddComponent(rhythmEngine, new RhythmEngineController {State      = RhythmEngineState.Playing, StartTime = worldTime.Total.Add(TimeSpan.FromSeconds(1))});
			AddComponent(rhythmEngine, new RhythmEngineSettings {BeatInterval = TimeSpan.FromSeconds(0.5), MaxBeat   = 4});
			AddComponent(rhythmEngine, new RhythmEngineLocalState());
			AddComponent(rhythmEngine, new RhythmEngineExecutingCommand());
			AddComponent(rhythmEngine, new Relative<PlayerDescription>(Safe(player)));
			GameWorld.AddComponent(rhythmEngine, GameWorld.AsComponentType<RhythmEngineCommandProgressBuffer>());
			GameWorld.AddComponent(rhythmEngine, GameWorld.AsComponentType<RhythmEnginePredictedCommandBuffer>());
			AddComponent(rhythmEngine, new GameCommandState());
			AddComponent(rhythmEngine, new SetRemoteAuthority<SimulationAuthority>());
			AddComponent(rhythmEngine, new NetworkedEntity());
			AddComponent(rhythmEngine, new OwnedNetworkedEntity(Safe(player)));
			GameCombo.AddToEntity(GameWorld, rhythmEngine);
			RhythmSummonEnergy.AddToEntity(GameWorld, rhythmEngine);

			GameWorld.RemoveComponent(character.Handle, AsComponentType<UnitFreeRoamMovement>());
			AddComponent(player, new GameRhythmInputComponent());
			AddComponent(player, new Relative<RhythmEngineDescription>(Safe(rhythmEngine)));
			AddComponent(rhythmEngine, new Relative<PlayerDescription>(Safe(player)));

			// make sure that we have a collection of entities in this frame, in case the character is instantly switched back to freeroam mode
			using var abilities = new PooledList<GameEntityHandle>
			{
				abilityCollectionSystem.SpawnFor("march", character.Handle),
				abilityCollectionSystem.SpawnFor("backward", character.Handle),
				abilityCollectionSystem.SpawnFor("retreat", character.Handle),
				abilityCollectionSystem.SpawnFor("jump", character.Handle)
			};
			abilities.ForEach(handle => AddComponent(handle, new NetworkedEntity()));

			var unitTarget = GameWorld.CreateEntity();
			GameWorld.AddComponent(unitTarget, new UnitTargetDescription());
			GameWorld.AddComponent(unitTarget, new Position());
			GameWorld.AddComponent(unitTarget, new UnitEnemySeekingState());
			GameWorld.AddComponent(unitTarget, new Relative<PlayerDescription>(Safe(player)));
			GameWorld.AddComponent(unitTarget, new NetworkedEntity());

			GameWorld.GetComponentData<Position>(unitTarget).Value.X = xPos;

			AddComponent(character, new Relative<UnitTargetDescription>(Safe(unitTarget)));
			AddComponent(character, new Relative<RhythmEngineDescription>(Safe(rhythmEngine)));
			AddComponent(character, new UnitTargetControlTag());
			AddComponent(character, new UnitTargetOffset());

			var displayedEquip = GameWorld.AddBuffer<UnitDisplayedEquipment>(character.Handle);
			displayedEquip.Add(new UnitDisplayedEquipment
			{
				Attachment = localAttachDb.GetOrCreate(resPathGen.Create(new[] {"equip_root", "mask"}, ResPath.EType.MasterServer)),
				Resource   = localEquipDb.GetOrCreate(resPathGen.Create(new[] {"equipments", "masks", "yarida"}, ResPath.EType.ClientResource))
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

			RequestWithAuthority<SimulationAuthority>(character, () =>
			{
				GetComponentData<Position>(character).Value.X = xPos;
				GetComponentData<UnitDirection>(character)    = direction;
			});
		}

		protected virtual void PlayLoop()
		{
			foreach (var player in playerQuery)
			{
				ref readonly var input = ref GetComponentData<FreeRoamInputComponent>(player);

				var reportTime = reportTimeSystem.Get(player, out var fromEntity);
				var character  = GetComponentData<PlayerFreeRoamCharacter>(player).Entity;
				if (input.Down.HasBeenPressed(reportTime.Active))
				{
					if (HasComponent<SetRemoteAuthority<SimulationAuthority>>(character.Handle))
					{
						Console.WriteLine("switch to server authority");
						GameWorld.AddRemoveMultipleComponent(
							character.Handle,
							new[] {AsComponentType<SimulationAuthority>()},
							new[] {AsComponentType<SetRemoteAuthority<SimulationAuthority>>()}
						);
					}
					else
					{
						Console.WriteLine("switch to client authority");
						GameWorld.AddRemoveMultipleComponent(
							character.Handle,
							new[] {AsComponentType<SetRemoteAuthority<SimulationAuthority>>()},
							new[] {AsComponentType<SimulationAuthority>()}
						);
					}
				}

				if (!HasComponent<Relative<RhythmEngineDescription>>(player))
				{
					var position = GetComponentData<Position>(character).Value.X;
					if (position > 8)
						CreateCharacterAt(10, character, unitTeam, UnitDirection.Left);
					else if (position < -8)
						CreateCharacterAt(-10, character, unitTeam, UnitDirection.Right);
				}
				else
				{
					if (input.Up.HasBeenPressed(reportTime.Active))
					{
						var rhythmEngine = GetComponentData<Relative<RhythmEngineDescription>>(character).Target;
						var unitTarget   = GetComponentData<Relative<UnitTargetDescription>>(character).Target;
						var abilities    = GetBuffer<OwnedRelative<AbilityDescription>>(character);
						
						GameWorld.RemoveEntity(rhythmEngine.Handle);
						{
							GameWorld.RemoveComponent(character.Handle, AsComponentType<Relative<RhythmEngineDescription>>());
							GameWorld.RemoveComponent(player, AsComponentType<Relative<RhythmEngineDescription>>());

							GameWorld.RemoveComponent(player, AsComponentType<GameRhythmInputComponent>());
						}
						GameWorld.RemoveEntity(unitTarget.Handle);
						{
							GameWorld.RemoveComponent(character.Handle, AsComponentType<Relative<UnitTargetDescription>>());
						}
						foreach (var relative in abilities)
						{
							GameWorld.RemoveEntity(relative.Target.Handle);
						}

						RequestWithAuthority<SimulationAuthority>(character, () =>
						{
							GetComponentData<Position>(character).Value.X = 0;
						});
						
						GameWorld.AddComponent(character.Handle, AsComponentType<UnitFreeRoamMovement>());
						
						GameWorld.GetBuffer<UnitDisplayedEquipment>(character.Handle).Clear();
					}
				}
			}
		}

		private void CreateBox(Vector3 pos)
		{
			var ent = CreateEntity();
			AddComponent(ent, new EnvironmentCollider());
			AddComponent(ent, new Position(pos));
			AddComponent(ent, new Relative<TeamDescription>(environmentTeam));

			var physicsSystem  = World.GetOrCreate(wc => new Box2DPhysicsSystem(wc));
			var entitySettings = World.Mgr.CreateEntity();
			entitySettings.Set<Shape>(new PolygonShape(4, 2));

			physicsSystem.AssignCollider(ent, entitySettings);
		}

		private GameEntity unitTeam;
		private GameEntity environmentTeam;
		protected override async Task GetStateMachine(CancellationToken token)
		{
			environmentTeam = Safe(CreateEntity());
			AddComponent(environmentTeam, new TeamDescription());
			AddComponent(environmentTeam, new SimulationAuthority());
			GameWorld.AddBuffer<TeamEntityContainer>(environmentTeam.Handle);
			
			unitTeam = Safe(CreateEntity());
			AddComponent(unitTeam, new TeamDescription());
			AddComponent(unitTeam, new SimulationAuthority());
			GameWorld.AddBuffer<TeamEntityContainer>(unitTeam.Handle);
			
			GameWorld.AddBuffer<TeamEnemies>(unitTeam.Handle).Add(new TeamEnemies(environmentTeam));
			GameWorld.AddBuffer<TeamEnemies>(environmentTeam.Handle).Add(new TeamEnemies(unitTeam));
			
			CreateBox(new Vector3(15, 0, 0));

			while (!token.IsCancellationRequested)
			{
				// Add missing input component to players
				foreach (var player in playerWithoutInputQuery.GetEnumerator())
				{
					AddComponent<FreeRoamInputComponent>(player);
					AddComponent<SetRemoteAuthority<InputAuthority>>(player);

					var character = Safe(unitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
					{
						Direction = UnitDirection.Right,
						Statistics = new UnitStatistics
						{
							BaseWalkSpeed       = 2,
							FeverWalkSpeed      = 2.2f,
							MovementAttackSpeed = 2.2f,
							Weight              = 8.5f,
							AttackSpeed         = 2f,
							AttackSeekRange     = 16f,
						}
					}));

					var archetype = resPathGen.Create(new[] {"archetype", "uberhero_std_unit"}, ResPath.EType.MasterServer);
					AddComponent(character, new UnitArchetype(localArchetypeDb.GetOrCreate(new UnitArchetypeResource(archetype))));
					
					var kit = "taterazay";
					AddComponent(character, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResource(kit))));

					AddComponent(player, new PlayerFreeRoamCharacter {Entity = character});
					
					AddComponent(character, new Owner(Safe(player)));
					AddComponent(character, new Relative<PlayerDescription>(Safe(player)));
					AddComponent(character, new Relative<TeamDescription>(unitTeam));
					AddComponent(character, new SetRemoteAuthority<SimulationAuthority>());
					//AddComponent(character, new SimulationAuthority());
					AddComponent(character, new NetworkedEntity());
					AddComponent(character, new OwnedNetworkedEntity(Safe(player)));

					GameWorld.Link(character.Handle, player, true);
				}

				PlayLoop();
				await Task.Yield();
			}

			// Remove input component from players
			foreach (var entity in playerWithInputQuery.GetEnumerator())
				GameWorld.RemoveComponent(entity, AsComponentType<FreeRoamInputComponent>());
		}
	}
}