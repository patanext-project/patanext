using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;

namespace StormiumTeam.GameBase.SystemBase
{
	public abstract class BaseProvider<TCreateData> : AppSystem
		where TCreateData : struct
	{
		protected GameWorld GameWorld;

		private PooledList<ComponentType> componentTypes;

		public BaseProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref GameWorld);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			componentTypes = new PooledList<ComponentType>();
			GetComponents(componentTypes);
		}

		public abstract void GetComponents(PooledList<ComponentType> entityComponents);

		public abstract void SetEntityData(GameEntityHandle entity, TCreateData data);

		public virtual void SpawnBatchEntitiesWithArguments(Span<TCreateData> array, Span<GameEntityHandle> outputEntities)
		{
			if (componentTypes.Count == 0)
				throw new InvalidOperationException("Invalid Provider (0 components)");

			GameWorld.CreateEntityBulk(outputEntities);
			for (var i = 0; i != outputEntities.Length; i++)
			{
				GameWorld.AddMultipleComponent(outputEntities[i], componentTypes.Span);
				SetEntityData(outputEntities[i], array[i]);
			}
		}

		public virtual GameEntityHandle SpawnEntityWithArguments(TCreateData data)
		{
			GameEntityHandle output = default;
			SpawnBatchEntitiesWithArguments(MemoryMarshal.CreateSpan(ref data, 1), MemoryMarshal.CreateSpan(ref output, 1));
			return output;
		}

		public void SpawnAndForget(TCreateData data)
		{
			SpawnEntityWithArguments(data);
		}
	}
}