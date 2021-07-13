using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.Components;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Utility;
using GameHost.Worlds.Components;
using PataNext.Game;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GameModes.City;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Game.Visuals;
using PataNext.Module.Simulation.Network.Snapshots;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Health.Systems;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Network.Components;
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

		private FreeRoamUnitProvider  unitProvider;
		private NetReportTimeSystem   reportTimeSystem;

		private AbilityCollectionSystem abilityCollectionSystem;

		private IManagedWorldTime worldTime;
		
		private ResPathGen resPathGen;
		
		GameResourceDb<UnitKitResource>       localKitDb;
		GameResourceDb<UnitArchetypeResource> localArchetypeDb;
		GameResourceDb<UnitAttachmentResource> localAttachDb;
		GameResourceDb<EquipmentResource> localEquipDb;
		GameResourceDb<GameGraphicResource> graphicDb;

		private MissionManager missionManager;
		
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
			DependencyResolver.Add(() => ref graphicDb);
			
			DependencyResolver.Add(() => ref missionManager);
			
			AddDisposable(World.Mgr.Subscribe(new MessageHandler<LaunchCoopMissionMessage>(onLaunchCoopMission)));
		}

		private void onLaunchCoopMission(in LaunchCoopMissionMessage message)
		{
			if (!missionManager.TryGet(message.Mission, out var missionEntity))
				throw new InvalidOperationException("No mission found with path " + message.Mission.FullString);
			
			if (GameWorld.TryGetSingleton<AtCityGameModeData>(out GameEntityHandle handle))
				GameWorld.RemoveEntity(handle);

			var newGameMode = GameWorld.CreateEntity();
			AddComponent(newGameMode, new CoopMission());
			AddComponent(newGameMode, new CoopMission.TargetMission { Entity = missionEntity });
		}

		private EntityQuery basePlayerQuery, playerWithoutInputQuery, playerWithInputQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			basePlayerQuery         = CreateEntityQuery(new[] {typeof(PlayerDescription)});
			playerWithInputQuery    = QueryWith(basePlayerQuery, new[] {typeof(FreeRoamInputComponent)});
			playerWithoutInputQuery = QueryWithout(basePlayerQuery, new[] {typeof(FreeRoamInputComponent)});
		}

		protected virtual void PlayLoop()
		{			
			foreach (var player in playerWithInputQuery)
			{
				ref readonly var input = ref GetComponentData<FreeRoamInputComponent>(player);
				
				var reportTime = reportTimeSystem.Get(player, out var fromEntity);
				var character  = GetComponentData<PlayerFreeRoamCharacter>(player).Entity;
			}
		}
		
		private GameEntity unitTeam;
		private GameEntity environmentTeam;
		protected override async Task GetStateMachine(CancellationToken token)
		{
			GameWorld.TryGetSingleton<AtCityGameModeData>(out GameEntityHandle gameModeEntity);
			
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

			using var temporaryEntities = new PooledList<GameEntityHandle>();
			while (!token.IsCancellationRequested)
			{
				// Add missing input component to players

				temporaryEntities.Clear();
				playerWithoutInputQuery.AddEntitiesTo(temporaryEntities);
				foreach (var player in playerWithoutInputQuery)
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

					var archetype = resPathGen.Create(new[] {"archetype", "uberhero_std_unit"}, ResPath.EType.MasterServer);
					AddComponent(character, new UnitArchetype(localArchetypeDb.GetOrCreate(new(archetype))));
					AddComponent(character, new SynchronizeCharacterVisual());
					
					var kit = "taterazay";
					AddComponent(character, new UnitCurrentKit(localKitDb.GetOrCreate(new(kit))));
					AddComponent(character, new UnitBodyCollider(1, 1.5f));

					AddComponent(player, new PlayerFreeRoamCharacter {Entity = character});
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

				PlayLoop();
				
				await Task.Yield();
			}

			// Remove input component from players
			foreach (var entity in playerWithInputQuery.GetEnumerator())
				GameWorld.RemoveComponent(entity, AsComponentType<FreeRoamInputComponent>());
		}
	}
}