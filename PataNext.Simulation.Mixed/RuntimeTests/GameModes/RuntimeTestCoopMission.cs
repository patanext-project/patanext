using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.Injection.Dependency;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using GameHost.Utility;
using JetBrains.Annotations;
using PataNext.Game;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Hideout;
using PataNext.Module.Simulation.GameModes;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Network.MasterServer.User;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.RuntimeTests.GameModes
{
	[DontInjectSystemToWorld] // instead it will get created
	public class RuntimeTestCoopMission : AppObject
	{
		public delegate void SetArmyUnit(int squad, int index, SafeEntityFocus unit);

		/// <summary>
		/// Default in Multiplayer.
		/// Consist of only an Uberhero.
		/// </summary>
		public static readonly int[] UberheroOnlyArmy = new[] { 1 };

		/// <summary>
		///	Default in Singleplayer.
		/// Consist of one Hatapon, one Uberhero, three squad composed of one Leader and 6 Soldiers.
		/// </summary>
		public static readonly int[] DefaultArmy = new[] { 1, 1, 7, 7 };

		GameResourceDb<UnitArchetypeResource> localArchetypeDb;
		GameResourceDb<UnitKitResource>       localKitDb;

		GameResourceDb<EquipmentResource>      equipDb;
		GameResourceDb<UnitAttachmentResource> attachDb;

		private ResPathGen    resPathGen;
		private IScheduler    scheduler;
		private TaskScheduler taskScheduler;

		private CurrentUserSystem currentUserSystem;

		private WorldCollection collection;
		private GameWorld       gameWorld;

		private readonly int[]       armyFormation;
		private readonly SetArmyUnit setArmyUnitDelegate;
		private readonly ResPath     missionPath;

		public RuntimeTestCoopMission([NotNull] WorldCollection collection, int[] armyFormation, SetArmyUnit setArmyUnitDelegate, ResPath missionPath) : base(collection.Ctx)
		{
			this.collection = collection;

			this.armyFormation       = armyFormation;
			this.setArmyUnitDelegate = setArmyUnitDelegate;
			this.missionPath         = missionPath;

			DependencyResolver.Add(() => ref localArchetypeDb);
			DependencyResolver.Add(() => ref localKitDb);
			DependencyResolver.Add(() => ref equipDb);
			DependencyResolver.Add(() => ref attachDb);

			DependencyResolver.Add(() => ref resPathGen);
			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref taskScheduler);

			DependencyResolver.Add(() => ref currentUserSystem);

			DependencyResolver.Add(() => ref gameWorld);

			DependencyResolver.OnComplete(OnDependenciesResolved);
		}

		private void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			var isInstant = armyFormation != null;
			if (isInstant)
				TestInstant();
			else
			{
				DependencyResolver.AddDependency(new TaskDependency(taskScheduler.StartUnwrap(async () =>
				{
					while (currentUserSystem.User.Token is null)
					{
						await Task.Delay(10);
					}
				})));
				DependencyResolver.OnComplete(_ => { TestWithMasterServer(); });
			}
		}

		private void TestWithMasterServer()
		{
			RequestUtility.CreateTracked(collection.Mgr, new GetFavoriteGameSaveRequest(), (Entity _, GetFavoriteGameSaveRequest.Response response) =>
			{
				var player = gameWorld.CreateEntity();

				gameWorld.AssureComponents(player, new[]
				{
					gameWorld.AsComponentType<PlayerDescription>(),
					gameWorld.AsComponentType<GameRhythmInputComponent>(),
					gameWorld.AsComponentType<PlayerIsLocal>(),
					gameWorld.AsComponentType<InputAuthority>(),
				});

				gameWorld.AddComponent(player, new PlayerAttachedGameSave(response.SaveId));

				var formation = gameWorld.CreateEntity();
				gameWorld.AddComponent(formation, new LocalArmyFormation());

				taskScheduler.StartUnwrap(async () =>
				{
					while (false == gameWorld.HasComponent<ArmyFormationDescription>(formation))
					{
						Console.WriteLine("???");
						await Task.Yield();
					}

					var squads = gameWorld.GetBuffer<OwnedRelative<ArmySquadDescription>>(formation);
					while (squads.Count == 0)
						await Task.Yield();

					foreach (var squad in squads)
					{
						var units = gameWorld.GetBuffer<OwnedRelative<ArmyUnitDescription>>(squad.Target.Handle);
						foreach (var unit in units)
						{
							while (!gameWorld.HasComponent<MasterServerIsUnitLoaded>(unit.Target.Handle))
								await Task.Yield();
						}
					}

					await Task.Delay(TimeSpan.FromSeconds(2));
					Console.WriteLine("start gamemode!");

					scheduler.Schedule(() =>
					{
						var gameMode = gameWorld.CreateEntity();
						gameWorld.AddComponent(gameMode, new CoopMission());
					}, default);
				});
			});
		}

		private void TestInstant()
		{
			var uberArchResource = localArchetypeDb.GetOrCreate(new UnitArchetypeResource(resPathGen.Create(new[] { "archetype", "uberhero_std_unit" }, ResPath.EType.MasterServer)));
			var kitResource      = localKitDb.GetOrCreate(new UnitKitResource("taterazay"));

			var player = gameWorld.CreateEntity();

			gameWorld.AssureComponents(player, new[]
			{
				gameWorld.AsComponentType<PlayerDescription>(),
				gameWorld.AsComponentType<GameRhythmInputComponent>(),
				gameWorld.AsComponentType<PlayerIsLocal>(),
				gameWorld.AsComponentType<InputAuthority>(),
			});

			gameWorld.AddComponent(player, gameWorld.AsComponentType<OwnedRelative<ArmySquadDescription>>());
			gameWorld.AddComponent(player, gameWorld.AsComponentType<OwnedRelative<ArmyUnitDescription>>());
			gameWorld.AddComponent(player, gameWorld.AsComponentType<OwnedRelative<UnitDescription>>());

			// ----
			// Create Army formation
			var formation = gameWorld.CreateEntity();
			gameWorld.AddComponent(formation, gameWorld.AsComponentType<ArmyFormationDescription>());
			gameWorld.AddComponent(formation, gameWorld.AsComponentType<OwnedRelative<ArmySquadDescription>>());
			for (var squadIdx = 0; squadIdx < armyFormation.Length; squadIdx++)
			{
				// ----
				// Create a squad of units, with the player being a relative.
				var squad = gameWorld.CreateEntity();
				gameWorld.AddComponent(squad, new Relative<ArmyFormationDescription>(gameWorld.Safe(formation)));
				gameWorld.AddComponent(squad, new Relative<PlayerDescription>(gameWorld.Safe(player)));
				gameWorld.AddComponent(squad, new ArmySquadDescription());
				gameWorld.AddComponent(squad, new Owner(gameWorld.Safe(formation)));

				gameWorld.GetBuffer<OwnedRelative<ArmySquadDescription>>(formation)
				         .Add(new(gameWorld.Safe(squad)));

				gameWorld.AddComponent(squad, gameWorld.AsComponentType<OwnedRelative<ArmyUnitDescription>>());
				for (var unitIdx = 0; unitIdx < armyFormation[squadIdx]; unitIdx++)
				{
					// ----
					// Add unit to army squad
					var unit = gameWorld.CreateEntity();
					gameWorld.AddComponent(unit, new Relative<ArmySquadDescription>(gameWorld.Safe(squad)));
					gameWorld.AddComponent(unit, new Relative<ArmyFormationDescription>(gameWorld.Safe(formation)));
					gameWorld.AddComponent(unit, new Relative<PlayerDescription>(gameWorld.Safe(player)));
					gameWorld.AddComponent(unit, new ArmyUnitDescription());
					gameWorld.AddComponent(unit, new Owner(gameWorld.Safe(squad)));

					gameWorld.AddComponent(unit, new UnitArchetype(uberArchResource));
					gameWorld.AddComponent(unit, new UnitCurrentKit(kitResource));

					gameWorld.AddComponent(unit, new UnitStatistics());

					gameWorld.AddBuffer<UnitDefinedAbilities>(unit);
					gameWorld.AddBuffer<UnitDisplayedEquipment>(unit);

					gameWorld.GetBuffer<OwnedRelative<ArmyUnitDescription>>(squad)
					         .Add(new(gameWorld.Safe(unit)));

					setArmyUnitDelegate(squadIdx, unitIdx, new(gameWorld, gameWorld.Safe(unit)));
				}
			}

			scheduler.Schedule(() =>
			{
				var gameMode = gameWorld.CreateEntity();
				gameWorld.AddComponent(gameMode, new CoopMission());

				var missionMgr = new ContextBindingStrategy(Context, true).Resolve<MissionManager>();
				if (!missionMgr.TryGet(missionPath, out var missionEntity))
					throw new InvalidOperationException("couldn't find mission");

				gameWorld.AddComponent(gameMode, new CoopMission.TargetMission
				{
					Entity = missionEntity
				});
			}, default);
		}
	}
}