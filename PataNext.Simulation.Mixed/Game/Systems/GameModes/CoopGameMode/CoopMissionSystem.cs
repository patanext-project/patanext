using System;
using System.Drawing;
using System.Threading.Tasks;
using BepuUtilities;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.IO;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using GameHost.Utility;
using GameHost.Worlds.Components;
using PataNext.Game;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Game.Visuals;
using PataNext.Module.Simulation.GameModes.DataCoopMission;
using PataNext.Module.Simulation.Network.Snapshots;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.GamePlay.Health.Providers;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;
using ZLogger;

namespace PataNext.Module.Simulation.GameModes
{
	public partial class CoopMissionSystem : MissionGameModeBase<CoopMission>
	{
		private IManagedWorldTime worldTime;
		
		private CoopMissionPlayerTeamProvider   teamProvider;
		private CoopMissionUnitTargetProvider   targetProvider;
		private CoopMissionRhythmEngineProvider rhythmEngineProvider;
		private CoopMissionPlayableUnitProvider unitProvider;
		private CoopMissionSquadProvider        squadProvider;
		private AbilityCollectionSystem         abilityCollection;

		private MissionManager missionManager;

		public CoopMissionSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
			
			DependencyResolver.Add(() => ref teamProvider);
			DependencyResolver.Add(() => ref targetProvider);
			DependencyResolver.Add(() => ref rhythmEngineProvider);
			DependencyResolver.Add(() => ref unitProvider);
			DependencyResolver.Add(() => ref squadProvider);
			DependencyResolver.Add(() => ref abilityCollection);
			
			DependencyResolver.Add(() => ref missionManager);
		}

		private GameEntity playerClub;
		private GameEntity playerTeam;

		protected override Task GameModeInitialisation()
		{
			var clubFocus = Focus(Safe(CreateEntity()));
			clubFocus.Add(AsComponentType<ClubDescription>())
			         .AddData(new ClubInformation
			         {
				         Name           = "Protagonists",
				         PrimaryColor   = Color.White,
				         SecondaryColor = Color.Black
			         });

			playerTeam = Safe(teamProvider.SpawnEntityWithArguments(new CreatePlayerTeam
			{
				OptionalClub = clubFocus.Entity
			}));

			return base.GameModeInitialisation();
		}

		private ResourceHandle<ScenarResource> scenarEntity;

		protected override async Task GameModeStartRound()
		{
			// Load Scenar
			var scenarRequest = World.Mgr.CreateEntity();
			scenarRequest.Set(new ScenarLoadRequest(new (ResPath.EType.ClientResource, "guerro", "test", "scenar")));

			scenarEntity = new ResourceHandle<ScenarResource>(scenarRequest);

			while (!scenarEntity.IsLoaded)
				await Task.Yield();

			var postComponentScheduler = new Scheduler();

			// Set Teams of players
			// Create Unit Targets
			// Create Rhythm Engines
			using var playerQuery = CreateEntityQuery(new[] {typeof(PlayerDescription)});
			
			playerQuery.ForEachDeferred(playerHandle =>
			{
				var target = targetProvider.SpawnEntityWithArguments(new CoopMissionUnitTargetProvider.Create
				{
					Direction = UnitDirection.Right,
					Player    = Safe(playerHandle),
					Team      = playerTeam
				});

				GetComponentData<Position>(target).Value.X -= 10;
				
				var rhythmEngine = rhythmEngineProvider.SpawnEntityWithArguments(new CoopMissionRhythmEngineProvider.Create
				{
					Base =
					{

					},
					Player = Safe(playerHandle)
				});
				
				AddComponent(playerHandle, new Relative<TeamDescription>(playerTeam));
				AddComponent(playerHandle, new Relative<UnitTargetDescription>(Safe(target)));
				AddComponent(playerHandle, new Relative<RhythmEngineDescription>(Safe(rhythmEngine)));
				
				GameWorld.AssureComponents(playerHandle, stackalloc [] { AsComponentType<OwnedRelative<UnitDescription>>() });
			}, postComponentScheduler);

			postComponentScheduler.Run();
			
			SpawnArmy();
			
			foreach (var player in playerQuery)
			{
				var rhythmEntity = GetComponentData<Relative<RhythmEngineDescription>>(player).Target;

				GetComponentData<RhythmEngineController>(rhythmEntity) = new RhythmEngineController
				{
					State     = RhythmEngineState.Playing,
					StartTime = worldTime.Total.Add(TimeSpan.FromSeconds(1))
				};
			}
			
			await scenarEntity.Result.Interface.StartAsync();
		}

		private EntityQuery damageEventQuery, livableQuery;
		protected override async Task GameModePlayLoop()
		{
			// Either the resource got destroyed or something worst happened
			if (!scenarEntity.IsLoaded)
			{
				RequestEndRound();
				return;
			}

			foreach (var pass in World.DefaultSystemCollection.Passes)
			{
				if (pass is not IGameEventPass.RegisterPass gameEvRegister)
					continue;

				gameEvRegister.Trigger();
			}
			
			using var evList = new PooledList<ModifyHealthEvent>();
			using var addList = new PooledList<GameEntityHandle>();
			foreach (var handle in damageEventQuery ??= CreateEntityQuery(new[]
			{
				typeof(TargetDamageEvent)
			}))
			{
				var ev = GetComponentData<TargetDamageEvent>(handle);
				evList.Add(new ModifyHealthEvent(ModifyHealthType.Add, (int) Math.Round(ev.Damage), ev.Victim));
			}

			while (evList.Count > 0)
			{
				World.TryGet(out ModifyHealthEventProvider healthEventProvider);
				healthEventProvider.SpawnAndForget(evList[0]);
				evList.RemoveAt(0);
			}
			
			foreach (var handle in livableQuery ??= CreateEntityQuery(new [] {typeof(LivableHealth)}, none: new [] {typeof(LivableIsDead)}))
			{
				var health = GetComponentData<LivableHealth>(handle);
				if (health.Value == 0 && health.Max > 0)
				{
					addList.Add(handle);

					if (TryGetComponentData(handle, out Velocity velocity) && TryGetComponentData(handle, out UnitDirection direction))
					{
						if (velocity.Value.X > direction.Value)
							velocity.Value.X = direction.Value;
						
						velocity.Value.X -= direction.Value * 7f;

						if (velocity.Value.Y < 0)
							velocity.Value.Y = 0;
						velocity.Value.Y += 5.5f;

						GetComponentData<Velocity>(handle) = velocity;
					}
				}
			}

			while (addList.Count > 0)
			{
				AddComponent(addList[0], new LivableIsDead());
				addList.RemoveAt(0);
			}

			await scenarEntity.Result.Interface.LoopAsync();
		}

		protected override async Task GameModeEndRound()
		{
			await base.GameModeEndRound();

			if (scenarEntity.IsLoaded)
			{
				await scenarEntity.Result.Interface.CleanupAsync(false);

				scenarEntity.Entity.Dispose();
			}

			scenarEntity = default;

			var postComponentScheduler = new Scheduler();
			
			GetBuffer<TeamAllies>(playerTeam).Clear();
			GetBuffer<TeamEnemies>(playerTeam).Clear();

			using var playerQuery = CreateEntityQuery(new[] {typeof(PlayerDescription)});
			playerQuery.ForEachDeferred(playerHandle =>
			{
				if (TryGetComponentData(playerHandle, out Relative<RhythmEngineDescription> rhythmEngine))
					RemoveEntity(rhythmEngine.Target);

				if (TryGetComponentData(playerHandle, out Relative<UnitTargetDescription> unitTarget))
					RemoveEntity(unitTarget.Target);

				GameWorld.RemoveComponent(playerHandle, AsComponentType<Relative<RhythmEngineDescription>>());
				GameWorld.RemoveComponent(playerHandle, AsComponentType<Relative<UnitTargetDescription>>());
				GameWorld.RemoveComponent(playerHandle, AsComponentType<Relative<TeamDescription>>());
			}, postComponentScheduler);

			using var unitQuery = CreateEntityQuery(new[] {typeof(UnitDescription)});
			unitQuery.RemoveAllEntities();

			postComponentScheduler.Run();
		}

		protected override Task GameModeCleanUp()
		{
			if (!RemoveEntity(playerTeam))
				Logger.ZLogWarning("A system has removed the playerTeam before us?");

			return base.GameModeCleanUp();
		}
	}

	public struct CoopMission : IComponentData
	{
		public struct TargetMission : IComponentData
		{
			public Entity Entity;
		}
	}
}