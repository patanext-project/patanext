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
		public GameEntityHandle CreateEntity()                 => GameWorld.CreateEntity();

		public ComponentType AsComponentType<T>() where T : struct, IEntityComponent => GameWorld.AsComponentType<T>();

		public bool HasComponent<T>(GameEntityHandle entity) where T : struct, IEntityComponent
		{
			return GameWorld.HasComponent<T>(entity);
		}

		public bool HasComponent(GameEntityHandle entity, ComponentType componentType)
		{
			return GameWorld.HasComponent(entity, componentType);
		}

		public ref T GetComponentData<T>(GameEntityHandle entity)
			where T : struct, IComponentData
		{
			return ref GameWorld.GetComponentData<T>(entity);
		}

		public ComponentBuffer<T> GetBuffer<T>(GameEntityHandle entity)
			where T : struct, IComponentBuffer
		{
			return GameWorld.GetBuffer<T>(entity);
		}

		public ComponentReference AddComponent<T>(GameEntityHandle entity, T data = default)
			where T : struct, IComponentData
		{
			return GameWorld.AddComponent(entity, data);
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

		public ComponentDataAccessor<T> GetAccessor<T>() where T : struct, IComponentData
		{
			return new ComponentDataAccessor<T>(GameWorld);
		}

		public ComponentBufferAccessor<T> GetBufferAccessor<T>() where T : struct, IComponentBuffer
		{
			return new ComponentBufferAccessor<T>(GameWorld);
		}

		public EntityQuery CreateEntityQuery(Span<Type> all = default, Span<Type> none = default)
		{
			if (GameWorld == null)
				throw new NullReferenceException(nameof(GameWorld));
			
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
			if (b == null)
				return CreateEntityQuery(span, Array.Empty<Type>());
			
			Span<ComponentType> convertedAll  = stackalloc ComponentType[b.All.Length + span.Length];
			b.All.CopyTo(convertedAll);
			
			for (var i = 0; i < span.Length; i++)
				convertedAll[b.All.Length + i] = GameWorld.AsComponentType(span[i]);
			return CreateEntityQuery(convertedAll, b.None);
		}

		public EntityQuery QueryWithout(EntityQuery b, Span<Type> span)
		{
			if (b == null)
				return CreateEntityQuery(Array.Empty<Type>(), span);
			
			Span<ComponentType> convertedNone = stackalloc ComponentType[b.None.Length + span.Length];
			b.None.CopyTo(convertedNone);

			for (var i = b.None.Length - 1; i < span.Length; i++)
				convertedNone[b.None.Length + i] = GameWorld.AsComponentType(span[i]);
			return CreateEntityQuery(b.All, convertedNone);
		}
	}
}