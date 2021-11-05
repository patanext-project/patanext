using System;
using System.Threading;
using System.Threading.Tasks;
using BepuPhysics;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.IO;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Game;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GameModes.City;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Network.Snapshots;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;

namespace PataNext.Module.Simulation.GameModes.InBasement
{
	public partial class AtCityGameModeSystem
	{
		private GameEntity environmentTeam;
		private GameEntity unitTeam;

		private void initializePlayer(GameEntityHandle gameModeEntity, GameEntityHandle player)
		{
			AddComponent<FreeRoamInputComponent>(player);
			AddComponent<SetRemoteAuthority<InputAuthority>>(player);

			var character = Safe(unitProvider.SpawnEntityWithArguments(new()
			{
				Direction = UnitDirection.Right,
				Statistics = new UnitStatistics
				{
					Attack              = 17,
					Health              = 300,
					BaseWalkSpeed       = 2,
					FeverWalkSpeed      = 2.2f,
					MovementAttackSpeed = 3.0f,
					Weight              = 7f,
					AttackSpeed         = 1.2f,
					AttackSeekRange     = 16f,

					AttackMeleeRange = 4f
				}
			}));

			var archetype = resPathGen.Create(new[] { "archetype", "uberhero_std_unit" }, ResPath.EType.MasterServer);
			AddComponent(character, new UnitArchetype(localArchetypeDb.GetOrCreate(new(archetype))));
			AddComponent(character, new SynchronizeCharacterVisual());

			var kit = "taterazay";
			AddComponent(character, new UnitCurrentKit(localKitDb.GetOrCreate(new(kit))));
			AddComponent(character, new UnitBodyCollider(1, 1.5f));

			AddComponent(player, new PlayerFreeRoamCharacter { Entity = character });
			AddComponent(player, new PlayerCurrentCityLocation());

			AddComponent(character, new Owner(Safe(player)));
			AddComponent(character, new Relative<PlayerDescription>(Safe(player)));
			AddComponent(character, new Relative<TeamDescription>(unitTeam));
			//AddComponent(character, new SetRemoteAuthority<SimulationAuthority>());
			AddComponent(character, new MovableAreaAuthority()); // this shouldn't be set on the client
			AddComponent(character, new SimulationAuthority());
			AddComponent(character, new NetworkedEntity());
			AddComponent(character, new OwnedNetworkedEntity(Safe(player)));

			GameWorld.AddComponent(player, new ServerCameraState
			{
				Data =
				{
					Mode   = CameraMode.Default,
					Offset = RigidPose.Identity,
					Target = character
				}
			});

			GameWorld.Link(character.Handle, player, true);
			GameWorld.Link(character.Handle, gameModeEntity, true);
		}

		private void cleanPlayer(GameEntityHandle player)
		{
			var character = GetComponentData<PlayerFreeRoamCharacter>(player).Entity;
			if (GameWorld.Exists(character))
				GameWorld.RemoveEntity(character.Handle);

			GameWorld.RemoveMultipleComponent(player, stackalloc ComponentType[]
			{
				AsComponentType<FreeRoamInputComponent>(),
				AsComponentType<SetRemoteAuthority<InputAuthority>>(),
				AsComponentType<PlayerCurrentCityLocation>()
			});
		}
		
		private ResourceHandle<ScenarResource> scenarResource;
		private GameEntity                     scenarGameEntity;

		private async ValueTask loadMissionAsync()
		{
			if (!TryGetComponentData(GetGameModeHandle(), out AtCityGameModeData.TargetMission targetMission))
				throw new InvalidOperationException("The gamemode should have a target mission");

			if (!targetMission.Target.TryGet(out MissionDetails missionDetails))
				throw new InvalidOperationException($"{targetMission.Target} has no {nameof(MissionDetails)} component");

			// Load Scenar
			var scenarRequest = World.Mgr.CreateEntity();
			scenarRequest.Set(new ScenarLoadRequest(missionDetails.Scenar));

			scenarResource = new ResourceHandle<ScenarResource>(scenarRequest);

			while (!scenarResource.IsLoaded)
				await Task.Yield();

			AddComponent(GetGameModeHandle(), new ExecutingMissionData(targetMission.Target));
			
			scenarGameEntity = Safe(CreateEntity());
			GameWorld.Link(scenarGameEntity.Handle, GetGameModeHandle(), true);
			
			await scenarResource.Result.Interface.StartAsync(scenarGameEntity, Safe(GetGameModeHandle()));
		}

		protected override async Task GetStateMachine(CancellationToken token)
		{
			var gameModeEntity = GetGameModeHandle();
			
			environmentTeam = Safe(CreateEntity());
			AddComponent(environmentTeam, new TeamDescription());
			AddComponent(environmentTeam, new SimulationAuthority());
			AddComponent(environmentTeam, new NetworkedEntity());
			AddComponent(environmentTeam, new TeamMovableArea());
			GameWorld.AddBuffer<TeamEntityContainer>(environmentTeam.Handle);
			GameWorld.AddBuffer<TeamAllies>(environmentTeam.Handle);

			unitTeam = Safe(CreateEntity());
			AddComponent(unitTeam, new TeamDescription());
			AddComponent(unitTeam, new SimulationAuthority());
			AddComponent(unitTeam, new NetworkedEntity());
			AddComponent(unitTeam, new TeamMovableArea());
			GameWorld.AddBuffer<TeamEntityContainer>(unitTeam.Handle);
			GameWorld.AddBuffer<TeamAllies>(unitTeam.Handle);

			GameWorld.AddBuffer<TeamEnemies>(unitTeam.Handle).Add(new(environmentTeam));
			GameWorld.AddBuffer<TeamEnemies>(environmentTeam.Handle).Add(new(unitTeam));

			var isPlayingGameModeType = AsComponentType<CityCurrentGameModeTarget>();
			var isSleepingType        = AsComponentType<IsInSleepingState>();
			var firstRun              = true;

			using var temporaryEntities = new PooledList<GameEntityHandle>();
			while (!token.IsCancellationRequested)
			{
				playerWithoutInputQuery.CheckForNewArchetypes();
				playerWithInputQuery.CheckForNewArchetypes();
				
				temporaryEntities.Clear();

				var shouldSleep = HasComponent(gameModeEntity, isPlayingGameModeType);
				switch (shouldSleep)
				{
					case true:
					{
						// Remove player components, and destroy city entities
						if (false == HasComponent(gameModeEntity, isSleepingType))
						{
							playerWithInputQuery.AddEntitiesTo(temporaryEntities);
							foreach (var player in temporaryEntities)
								cleanPlayer(player);

							GameWorld.AddComponent(gameModeEntity, isSleepingType);

							if (scenarResource.IsLoaded)
							{
								await scenarResource.Result.Interface.CleanupAsync(false);
							}

							RemoveEntity(scenarGameEntity);
						}

						var targetGameMode = GetComponentData<CityCurrentGameModeTarget>(gameModeEntity).Entity;
						if (!GameWorld.Exists(targetGameMode)
						    || HasComponent<GameModeIsDisposedTag>(targetGameMode))
						{
							RemoveEntity(targetGameMode);
							GameWorld.RemoveComponent(gameModeEntity, isPlayingGameModeType);
						}

						break;
					}

					// The gamemode shouldn't sleep.
					// If we were in a sleeping state then we need to load old data TODO
					case false:
					{
						if (HasComponent(gameModeEntity, isSleepingType) || firstRun)
						{
							// TODO: Load previous city data (unit positions, etc...)

							// Load city map/mission with scenar
							await loadMissionAsync();

							GameWorld.RemoveComponent(gameModeEntity, isSleepingType);
						}

						// Add missing input component to players
						playerWithoutInputQuery.AddEntitiesTo(temporaryEntities);
						foreach (var player in temporaryEntities)
							initializePlayer(gameModeEntity, player);

						await scenarResource.Result.Interface.LoopAsync();
						PlayLoop();
						break;
					}
				}

				await Task.Yield();

				firstRun = false;
			}

			// Remove input component from players
			foreach (var entity in playerWithInputQuery.GetEnumerator())
				GameWorld.RemoveComponent(entity, AsComponentType<FreeRoamInputComponent>());
		}
	}
}