using System;
using System.Runtime.InteropServices;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;

namespace PataNext.Module.Simulation.GameBase.SystemBase
{
	public abstract class BaseProvider<TCreateData> : AppSystem
		where TCreateData : struct
	{
		protected GameWorld GameWorld;

		public BaseProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref GameWorld);
		}
		
		public abstract void GetComponents(PooledList<ComponentType> entityComponents);

		public abstract void SetEntityData(GameEntity entity, TCreateData data);

		public virtual void SpawnBatchEntitiesWithArguments(Span<TCreateData> array, Span<GameEntity> outputEntities)
		{
			GameWorld.CreateEntityBulk(outputEntities);
			for (var i = 0; i != outputEntities.Length; i++)
			{
				SetEntityData(outputEntities[i], array[i]);
			}
		}

		public virtual GameEntity SpawnEntityWithArguments(TCreateData data)
		{
			GameEntity output = default;
			SpawnBatchEntitiesWithArguments(MemoryMarshal.CreateSpan(ref data, 1), MemoryMarshal.CreateSpan(ref output, 1));
			return output;
		}
	}
}