using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;

namespace StormiumTeam.GameBase.SystemBase
{
	public class GameAppSystem : AppSystem
	{
		private GameWorld gameWorld;

		public GameAppSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref gameWorld);
		}

		public GameWorld GameWorld => gameWorld;

		public GameEntity CreateEntity() => GameWorld.CreateEntity();

		public ComponentType AsComponentType<T>() where T : struct, IEntityComponent => GameWorld.AsComponentType<T>();

		public bool HasComponent<T>(GameEntity entity) where T : struct, IEntityComponent
		{
			return GameWorld.HasComponent<T>(entity);
		}

		public bool HasComponent(GameEntity entity, ComponentType componentType)
		{
			return GameWorld.HasComponent(entity, componentType);
		}

		public ref T GetComponentData<T>(GameEntity entity)
			where T : struct, IComponentData
		{
			return ref GameWorld.GetComponentData<T>(entity);
		}

		public ComponentReference AddComponent<T>(GameEntity entity, T data = default)
			where T : struct, IComponentData
		{
			return GameWorld.AddComponent(entity, data);
		}

		public bool TryGetComponentData<T>(GameEntity entity, out T result, T defaultData = default)
			where T : struct, IComponentData
		{
			if (HasComponent<T>(entity))
			{
				result = GetComponentData<T>(entity);
				return true;
			}

			result = defaultData;
			return false;
		}

		public ComponentDataAccessor<T> GetAccessor<T>() where T : struct, IComponentData
		{
			return new ComponentDataAccessor<T>(GameWorld);
		}

		public EntityQuery CreateEntityQuery(Span<Type> all = default, Span<Type> none = default)
		{
			Span<ComponentType> convertedAll  = stackalloc ComponentType[all.Length];
			Span<ComponentType> convertedNone = stackalloc ComponentType[none.Length];
			for (var i = 0; i != all.Length; i++)
				convertedAll[i] = GameWorld.AsComponentType(all[i]);
			for (var i = 0; i != none.Length; i++)
				convertedNone[i] = GameWorld.AsComponentType(none[i]);

			return CreateEntityQuery(convertedAll, convertedNone);
		}

		public EntityQuery CreateEntityQuery(Span<ComponentType> all = default, Span<ComponentType> none = default)
		{
			var query = new EntityQuery(GameWorld, new FinalizedQuery {All = all, None = none});
			AddDisposable(query);

			return query;
		}

		public EntityQuery QueryWith(EntityQuery b, Span<Type> span)
		{
			Span<ComponentType> convertedAll  = stackalloc ComponentType[b.All.Length + span.Length];
			b.All.CopyTo(convertedAll);
			
			for (var i = b.All.Length - 1; i < span.Length; i++)
				convertedAll[i] = GameWorld.AsComponentType(span[i]);
			return CreateEntityQuery(convertedAll, b.None);
		}

		public EntityQuery QueryWithout(EntityQuery b, Span<Type> span)
		{
			Span<ComponentType> convertedNone = stackalloc ComponentType[b.None.Length + span.Length];
			b.None.CopyTo(convertedNone);

			for (var i = b.None.Length - 1; i < span.Length; i++)
				convertedNone[i] = GameWorld.AsComponentType(span[i]);
			return CreateEntityQuery(b.All, convertedNone);
		}
	}
}