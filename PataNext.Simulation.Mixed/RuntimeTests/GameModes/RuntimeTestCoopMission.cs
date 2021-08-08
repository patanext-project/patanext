using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.Injection.Dependency;
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
	public class RuntimeTestCoopMission : GameAppSystem
	{
		GameResourceDb<UnitArchetypeResource> localArchetypeDb;
		GameResourceDb<UnitKitResource> localKitDb;
		
		GameResourceDb<EquipmentResource> equipDb;
		GameResourceDb<UnitAttachmentResource> attachDb;
		
		private ResPathGen    resPathGen;
		private IScheduler    scheduler;
		private TaskScheduler taskScheduler;

		private CurrentUserSystem currentUserSystem;

		public RuntimeTestCoopMission([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref localArchetypeDb);
			DependencyResolver.Add(() => ref localKitDb);
			DependencyResolver.Add(() => ref equipDb);
			DependencyResolver.Add(() => ref attachDb);
			
			DependencyResolver.Add(() => ref resPathGen);
			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref taskScheduler);
			
			DependencyResolver.Add(() => ref currentUserSystem);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			var isInstant = true;
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
				DependencyResolver.OnComplete(_ =>
				{
					TestWithMasterServer();
				});
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
		}

		private void TestWithMasterServer()
		{
			RequestUtility.CreateTracked(World.Mgr, new GetFavoriteGameSaveRequest(), (Entity _, GetFavoriteGameSaveRequest.Response response) =>
			{
				var player = CreateEntity();
				AddComponent(Safe(player),
					new PlayerDescription(),
					new GameRhythmInputComponent(),
					new PlayerIsLocal(),
					new InputAuthority()
				);

				AddComponent(player, new PlayerAttachedGameSave(response.SaveId));

				var formation = CreateEntity();
				AddComponent(formation, new LocalArmyFormation());

				taskScheduler.StartUnwrap(async () =>
				{
					while (false == HasComponent<ArmyFormationDescription>(formation))
					{
						Console.WriteLine("???");
						await Task.Yield();
					}

					var squads = GetBuffer<OwnedRelative<ArmySquadDescription>>(formation);
					while (squads.Count == 0)
						await Task.Yield();
					
					foreach (var squad in squads)
					{
						var units = GetBuffer<OwnedRelative<ArmyUnitDescription>>(squad.Target);
						foreach (var unit in units)
						{
							while (!HasComponent<MasterServerIsUnitLoaded>(unit.Target))
								await Task.Yield();
						}
					}

					await Task.Delay(TimeSpan.FromSeconds(2));
					Console.WriteLine("start gamemode!");

					scheduler.Schedule(() =>
					{
						var gameMode = GameWorld.CreateEntity();
						GameWorld.AddComponent(gameMode, new CoopMission());
					}, default);
				});
			});
		}

		private void TestInstant()
		{
			var uberArchResource = localArchetypeDb.GetOrCreate(new UnitArchetypeResource(resPathGen.Create(new[] { "archetype", "uberhero_std_unit" }, ResPath.EType.MasterServer)));
			var kitResource      = localKitDb.GetOrCreate(new UnitKitResource("wondabarappa"));

			var player = CreateEntity();
			AddComponent(Safe(player),
				new PlayerDescription(),
				new GameRhythmInputComponent(),
				new PlayerIsLocal(),
				new InputAuthority()
			);
			GameWorld.AddComponent(player, AsComponentType<OwnedRelative<ArmySquadDescription>>());
			GameWorld.AddComponent(player, AsComponentType<OwnedRelative<ArmyUnitDescription>>());
			GameWorld.AddComponent(player, AsComponentType<OwnedRelative<UnitDescription>>());

			// ----
			// Create Army formation
			var formation = CreateEntity();
			AddComponent(Safe(formation),
				new ArmyFormationDescription());
			GameWorld.AddComponent(formation, AsComponentType<OwnedRelative<ArmySquadDescription>>());
			{
				// ----
				// Create a squad of units, with the player being a relative.
				var squad = CreateEntity();
				AddComponent(Safe(squad),
					new Relative<ArmyFormationDescription>(Safe(formation)),
					new Relative<PlayerDescription>(Safe(player)),
					new ArmySquadDescription(),
					new Owner(Safe(formation)));
				
				GetBuffer<OwnedRelative<ArmySquadDescription>>(formation)
					.Add(new(Safe(squad)));
				
				GameWorld.AddComponent(squad, AsComponentType<OwnedRelative<ArmyUnitDescription>>());
				{
					// ----
					// Add unit to army squad
					var unit = CreateEntity();
					AddComponent(Safe(unit),
						new Relative<ArmySquadDescription>(Safe(squad)),
						new Relative<ArmyFormationDescription>(Safe(formation)),
						new Relative<PlayerDescription>(Safe(player)),
						new ArmyUnitDescription(),
						new Owner(Safe(squad)),

						new UnitArchetype(uberArchResource),
						new UnitCurrentKit(kitResource),
						new UnitTargetControlTag()
					);

					AddComponent(unit, new UnitStatistics());

					var abilityBuffer  = GameWorld.AddBuffer<UnitDefinedAbilities>(unit);
					var displayedEquip = GameWorld.AddBuffer<UnitDisplayedEquipment>(unit);
					
					GetBuffer<OwnedRelative<ArmyUnitDescription>>(squad)
						.Add(new(Safe(unit)));
				}
			}

			scheduler.Schedule(() =>
			{
				var gameMode = GameWorld.CreateEntity();
				GameWorld.AddComponent(gameMode, new CoopMission());

				var missionMgr = new ContextBindingStrategy(Context, true).Resolve<MissionManager>();
				if (!missionMgr.TryGet(resPathGen.GetDefaults().With(new[] { "mission", "debug" }, ResPath.EType.MasterServer), out var missionEntity))
					throw new InvalidOperationException("couldn't find mission");

				GameWorld.AddComponent(gameMode, new CoopMission.TargetMission
				{
					Entity = missionEntity
				});
			}, default);
		}
	}
}