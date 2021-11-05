using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Time.Components;

namespace StormiumTeam.GameBase.Time
{
	public class RemoveEntityWithEndTimeSystem : GameAppSystem, IPreUpdateSimulationPass
	{
		private IManagedWorldTime worldTime;

		public RemoveEntityWithEndTimeSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery                  entityQuery;
		private PooledList<GameEntityHandle> toRemove = new();

		public void OnBeforeSimulationUpdate()
		{
			toRemove.Clear();

			var asStruct  = worldTime.ToStruct();
			var component = GetAccessor<RemoveEntityEndTime>();
			foreach (var entity in entityQuery ??= CreateEntityQuery(new[]
			{
				typeof(RemoveEntityEndTime)
			}))
			{
				if (component[entity].Value <= asStruct.Total)
					toRemove.Add(entity);
			}

			var length = toRemove.Count;
			var span   = toRemove.Span;
			for (var i = 0; i < length; i++)
				GameWorld.RemoveEntity(span[i]);
		}
	}
}