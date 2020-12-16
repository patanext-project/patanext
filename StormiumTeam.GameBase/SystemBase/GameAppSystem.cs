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

		public GameEntity       Safe(GameEntityHandle handle) => GameWorld.Safe(handle);
		public GameEntityHandle CreateEntity()                => GameWorld.CreateEntity();
		public TemporaryEntity  CreateTemporary()             => new(gameWorld);

		public ComponentType AsComponentType<T>() where T : struct, IEntityComponent => GameWorld.AsComponentType<T>();

		public bool HasComponent<T>(GameEntityHandle entity) where T : struct, IEntityComponent
		{
			return GameWorld.HasComponent<T>(entity);
		}

		public bool HasComponent(GameEntityHandle entity, ComponentType componentType)
		{
			return GameWorld.HasComponent(entity, componentType);
		}

		public T GetComponentDataOrDefault<T>(GameEntityHandle entity, T def = default)
			where T : struct, IComponentData
		{
			if (!GameWorld.Contains(entity) || !HasComponent<T>(entity))
				return def;
			return GetComponentData<T>(entity);
		}

		public ref T GetComponentData<T>(GameEntityHandle entity)
			where T : struct, IComponentData
		{
			return ref GameWorld.GetComponentData<T>(entity);
		}

		public ref T GetComponentData<T>(GameEntity entity)
			where T : struct, IComponentData
		{
			return ref GameWorld.GetComponentData<T>(entity.Handle);
		}

		public ComponentBuffer<T> GetBuffer<T>(GameEntityHandle entity)
			where T : struct, IComponentBuffer
		{
			return GameWorld.GetBuffer<T>(entity);
		}

		public ComponentBuffer<T> GetBuffer<T>(GameEntity entity)
			where T : struct, IComponentBuffer
		{
			return GameWorld.GetBuffer<T>(entity.Handle);
		}

		public ComponentReference AddComponent<T>(GameEntityHandle entity, T data = default)
			where T : struct, IComponentData
		{
			return GameWorld.AddComponent(entity, data);
		}

		public ComponentReference AddComponent<T>(GameEntity entity, T data = default)
			where T : struct, IComponentData
		{
			return GameWorld.AddComponent(entity.Handle, data);
		}

		public bool TryGetComponentData<T>(GameEntityHandle entity, out T result, T defaultData = default)
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

		public bool TryGetComponentData<T>(GameEntity entity, out T result, T defaultData = default)
			where T : struct, IComponentData
		{
			return TryGetComponentData(entity.Handle, out result, defaultData);
		}

		public bool TryGetComponentBuffer<T>(GameEntityHandle entity, out ComponentBuffer<T> result)
			where T : struct, IComponentBuffer
		{
			if (HasComponent<T>(entity))
			{
				result = GetBuffer<T>(entity);
				return true;
			}

			result = default;
			return false;
		}

		public bool TryGetComponentBuffer<T>(GameEntity entity, out ComponentBuffer<T> result)
			where T : struct, IComponentBuffer
		{
			return TryGetComponentBuffer(entity.Handle, out result);
		}

		public ComponentDataAccessor<T> GetAccessor<T>() where T : struct, IComponentData
		{
			return new ComponentDataAccessor<T>(GameWorld);
		}

		public ComponentBufferAccessor<T> GetBufferAccessor<T>() where T : struct, IComponentBuffer
		{
			return new ComponentBufferAccessor<T>(GameWorld);
		}

		public EntityQuery CreateEntityQuery(Span<Type> all = default, Span<Type> none = default, Span<Type> or = default)
		{
			if (GameWorld == null)
				throw new NullReferenceException(nameof(GameWorld));

			Span<ComponentType> convertedAll  = stackalloc ComponentType[all.Length];
			Span<ComponentType> convertedNone = stackalloc ComponentType[none.Length];
			Span<ComponentType> convertedOr   = stackalloc ComponentType[or.Length];
			for (var i = 0; i != all.Length; i++)
				convertedAll[i] = GameWorld.AsComponentType(all[i]);
			for (var i = 0; i != none.Length; i++)
				convertedNone[i] = GameWorld.AsComponentType(none[i]);
			for (var i = 0; i != or.Length; i++)
				convertedOr[i] = GameWorld.AsComponentType(or[i]);

			return CreateEntityQuery(convertedAll, convertedNone, convertedOr);
		}

		public EntityQuery CreateEntityQuery(Span<ComponentType> all = default, Span<ComponentType> none = default, Span<ComponentType> or = default)
		{
			var query = new EntityQuery(GameWorld, new FinalizedQuery {All = all, None = none, Or = or});
			AddDisposable(query);

			return query;
		}

		public EntityQuery QueryWith(EntityQuery b, Span<Type> span)
		{
			if (b == null)
				return CreateEntityQuery(span, Array.Empty<Type>());

			Span<ComponentType> convertedAll = stackalloc ComponentType[b.All.Length + span.Length];
			b.All.CopyTo(convertedAll);

			for (var i = 0; i < span.Length; i++)
				convertedAll[b.All.Length + i] = GameWorld.AsComponentType(span[i]);
			return CreateEntityQuery(convertedAll, b.None, b.Or);
		}

		public EntityQuery QueryWithout(EntityQuery b, Span<Type> span)
		{
			if (b == null)
				return CreateEntityQuery(Array.Empty<Type>(), span);

			Span<ComponentType> convertedNone = stackalloc ComponentType[b.None.Length + span.Length];
			b.None.CopyTo(convertedNone);

			for (var i = 0; i < span.Length; i++)
				convertedNone[b.None.Length + i] = GameWorld.AsComponentType(span[i]);
			return CreateEntityQuery(b.All, convertedNone, b.Or);
		}

		public EntityQuery QueryOr(EntityQuery b, Span<Type> span)
		{
			if (b == null)
				return CreateEntityQuery(Array.Empty<Type>(), Array.Empty<Type>(), span);

			Span<ComponentType> convertedOr = stackalloc ComponentType[b.Or.Length + span.Length];
			b.Or.CopyTo(convertedOr);

			for (var i = 0; i < span.Length; i++)
				convertedOr[b.Or.Length + i] = GameWorld.AsComponentType(span[i]);
			return CreateEntityQuery(b.All, b.None, b.Or);
		}
	}
}