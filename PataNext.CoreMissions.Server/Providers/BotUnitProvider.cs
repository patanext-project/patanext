using System.Numerics;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Game.Visuals;
using PataNext.Module.Simulation.Network.Snapshots;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.GamePlay.Health.Systems;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreMissions.Server.Providers
{
	public class BotUnitProvider : BaseProvider<BotUnitProvider.Create>
	{
		public struct Create
		{
			public UnitDirection  Direction;
			public UnitStatistics Statistics;

			public Vector2                           InitialPosition;
			public GameResource<GameGraphicResource> Visual;

			public UnitBodyCollider Collider;

			public bool UseCustomHealthProvider;
			public int  HealthBegin;

			public float? CustomGravity;
		}

		private PlayableUnitProvider  parent;
		private DefaultHealthProvider healthProvider;

		public BotUnitProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref parent);
			DependencyResolver.Add(() => ref healthProvider);
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			parent.GetComponents(entityComponents);

			entityComponents.AddRange(new[]
			{
				AsComponentType<EntityVisual>(),
				AsComponentType<SimulationAuthority>(),
				AsComponentType<MovableAreaAuthority>(),
				AsComponentType<UnitBodyCollider>(),

				AsComponentType<UnitEnemySeekingState>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			parent.SetEntityData(entity, new() { Direction = data.Direction, Statistics = data.Statistics });

			GetComponentData<Position>(entity).Value   = new(data.InitialPosition, 0);
			GetComponentData<EntityVisual>(entity)     = new(data.Visual, true);
			GetComponentData<UnitBodyCollider>(entity) = data.Collider;

			if (data.UseCustomHealthProvider == false)
			{
				healthProvider.SpawnEntityWithArguments(new()
				{
					value = data.HealthBegin,
					max   = data.Statistics.Health,
					owner = Safe(entity)
				});

				GetComponentData<LivableHealth>(entity) = new()
				{
					Value = data.HealthBegin,
					Max   = data.Statistics.Health
				};
			}

			if (data.CustomGravity is { } gravity)
			{
			}
		}
	}
}