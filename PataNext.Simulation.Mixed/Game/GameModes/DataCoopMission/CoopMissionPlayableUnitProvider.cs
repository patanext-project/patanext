using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameModes.DataCoopMission
{
	public class CoopMissionPlayableUnitProvider : BaseProvider<CoopMissionPlayableUnitProvider.Create>
	{
		public struct Create
		{
			public PlayableUnitProvider.Create Base;

			public GameEntity Team,
			                  Player,
			                  UnitTarget,
			                  RhythmEngine;
		}

		private PlayableUnitProvider parent;

		public CoopMissionPlayableUnitProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref parent);
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			parent.GetComponents(entityComponents);

			entityComponents.AddRange(new[]
			{
				AsComponentType<Owner>(),
				
				AsComponentType<Relative<TeamDescription>>(),
				AsComponentType<Relative<PlayerDescription>>(),
				AsComponentType<Relative<UnitTargetDescription>>(),
				AsComponentType<Relative<RhythmEngineDescription>>(),

				AsComponentType<UnitEnemySeekingState>(),
				AsComponentType<UnitBodyCollider>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			parent.SetEntityData(entity, data.Base);

			if (!GameWorld.Exists(data.Player)) throw new InvalidOperationException($"Player Entity {data.Player} does not exist");
			if (!GameWorld.Exists(data.Team)) throw new InvalidOperationException($"Team Entity {data.Team} does not exist");
			if (!GameWorld.Exists(data.UnitTarget)) throw new InvalidOperationException($"UnitTarget Entity {data.UnitTarget} does not exist");
			if (!GameWorld.Exists(data.RhythmEngine)) throw new InvalidOperationException($"RhythmEngine Entity {data.RhythmEngine} does not exist");

			GetComponentData<Owner>(entity) = new Owner(data.Player);

			GetComponentData<Relative<TeamDescription>>(entity)         = new Relative<TeamDescription>(data.Team);
			GetComponentData<Relative<PlayerDescription>>(entity)       = new Relative<PlayerDescription>(data.Player);
			GetComponentData<Relative<UnitTargetDescription>>(entity)   = new Relative<UnitTargetDescription>(data.UnitTarget);
			GetComponentData<Relative<RhythmEngineDescription>>(entity) = new Relative<RhythmEngineDescription>(data.RhythmEngine);

			GetComponentData<UnitBodyCollider>(entity) = new UnitBodyCollider(1, 1.5f);
		}
	}
}