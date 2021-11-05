using System.Numerics;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Structures;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Game.Visuals;
using PataNext.Module.Simulation.Network.Snapshots;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.GamePlay.Health.Systems;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.Providers
{
	public class SimpleDestroyableStructureProvider : BaseProvider<SimpleDestroyableStructureProvider.Create>
	{
		public struct Create
		{
			public int                               Health;
			public Vector3                           Position;
			public GameResource<GameGraphicResource> Visual;
			public Entity                            ColliderDefinition;

			public ContributeToTeamMovableArea Area;
		}

		private DefaultHealthProvider healthProvider;
		private IPhysicsSystem        physicsSystem;

		public SimpleDestroyableStructureProvider([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref healthProvider);
			DependencyResolver.Add(() => ref physicsSystem);
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				AsComponentType<DestroyableStructureDescription>(),
				AsComponentType<LivableHealth>(),
				AsComponentType<EntityVisual>(),
				AsComponentType<Position>(),
				AsComponentType<ContributeToTeamMovableArea>(),
				AsComponentType<MovableAreaAuthority>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			GetComponentData<Position>(entity).Value = data.Position;
			GetComponentData<EntityVisual>(entity)   = new EntityVisual(data.Visual);

			healthProvider.SpawnEntityWithArguments(new DefaultHealthProvider.Create
			{
				value = data.Health,
				max   = data.Health,
				owner = Safe(entity)
			});
			GetComponentData<LivableHealth>(entity)               = new LivableHealth {Value = data.Health, Max = data.Health};
			GetComponentData<ContributeToTeamMovableArea>(entity) = data.Area;

			physicsSystem.AssignCollider(entity, data.ColliderDefinition);
		}
	}
}