using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.Components;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Game;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GameModes.City;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameModes.InBasement
{
	public partial class AtCityGameModeSystem : GameModeSystemBase<AtCityGameModeData>
	{
		private AbilityCollectionSystem abilityCollectionSystem;

		private EntityQuery                            basePlayerQuery, playerWithoutInputQuery, playerWithInputQuery;
		private GameResourceDb<GameGraphicResource>    graphicDb;
		private GameResourceDb<UnitArchetypeResource>  localArchetypeDb;
		private GameResourceDb<UnitAttachmentResource> localAttachDb;
		private GameResourceDb<EquipmentResource>      localEquipDb;

		private GameResourceDb<UnitKitResource> localKitDb;

		private MissionManager      missionManager;
		private NetReportTimeSystem reportTimeSystem;

		private ResPathGen resPathGen;

		private FreeRoamUnitProvider unitProvider;

		private IManagedWorldTime worldTime;

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

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			basePlayerQuery         = CreateEntityQuery(new[] { typeof(PlayerDescription) });
			playerWithInputQuery    = QueryWith(basePlayerQuery, new[] { typeof(FreeRoamInputComponent) });
			playerWithoutInputQuery = QueryWithout(basePlayerQuery, new[] { typeof(FreeRoamInputComponent) });
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

		public struct PlayerFreeRoamCharacter : IComponentData
		{
			public GameEntity Entity;
		}

		public struct IsInSleepingState : IComponentData
		{
		}
	}
}