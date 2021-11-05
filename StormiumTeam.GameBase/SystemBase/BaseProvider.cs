using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.SystemBase
{
	public abstract class BaseProvider : GameAppSystem
	{
		internal PooledList<ComponentType> componentTypes;

		protected BaseProvider(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			componentTypes = new();
			GetComponents(componentTypes);
		}

		public abstract void GetComponents(PooledList<ComponentType> entityComponents);

		public abstract void             SpawnBatchEntities(Span<Entity> dentArray, Span<GameEntityHandle> outputEntities);
		public abstract GameEntityHandle SpawnEntity(Entity              dent);
	}

	public abstract class BaseProvider<TCreateData> : BaseProvider
		where TCreateData : struct
	{
		public BaseProvider(WorldCollection collection) : base(collection)
		{
		}

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

		public override void SpawnBatchEntities(Span<Entity> dentArray, Span<GameEntityHandle> outputEntities)
		{
			var array = ArrayPool<TCreateData>.Shared.Rent(dentArray.Length);
			try
			{
				var span = array.AsSpan(0, dentArray.Length);
				for (var i = 0; i < dentArray.Length; i++)
					span[i] = dentArray[i].Get<TCreateData>();

				SpawnBatchEntitiesWithArguments(span, outputEntities);
			}
			finally
			{
				ArrayPool<TCreateData>.Shared.Return(array);
			}
		}

		public virtual GameEntityHandle SpawnEntityWithArguments(TCreateData data)
		{
			GameEntityHandle output = default;
			SpawnBatchEntitiesWithArguments(MemoryMarshal.CreateSpan(ref data, 1), MemoryMarshal.CreateSpan(ref output, 1));
			return output;
		}

		public override GameEntityHandle SpawnEntity(Entity dent)
		{
			return SpawnEntityWithArguments(dent.Get<TCreateData>());
		}

		public void SpawnAndForget(TCreateData data)
		{
			SpawnEntityWithArguments(data);
		}
	}
}