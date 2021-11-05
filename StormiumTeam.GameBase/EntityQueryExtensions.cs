using System;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;

namespace StormiumTeam.GameBase
{
	public static class EntityQueryExtensions
	{
		private static readonly Action<(GameEntityHandle handle, Action<GameEntityHandle> action)> ForEachDeferredDelegate = args => { args.action(args.handle); };

		private static class WithArgs<T>
		{
			public static readonly Action<(GameEntityHandle handle, Action<GameEntityHandle, T> action, T args)> ForEachDeferredDelegate = args => { args.action(args.handle, args.args); };
		}

		public static void ForEachDeferred<TScheduler>(this EntityQuery query, Action<GameEntityHandle> onEntity, TScheduler scheduler,
		                                               bool             checkIfEntityExist = false)
			where TScheduler : IScheduler
		{
			foreach (var handle in query)
			{
				scheduler.Schedule(ForEachDeferredDelegate, (handle, onEntity), default);
			}
		}

		public static void ForEachDeferred<TScheduler, TArgs>(this EntityQuery query, Action<GameEntityHandle, TArgs> onEntity, TArgs args, TScheduler scheduler,
		                                                      bool             checkIfEntityExist = false)
			where TScheduler : IScheduler
		{
			foreach (var handle in query)
			{
				scheduler.Schedule(WithArgs<TArgs>.ForEachDeferredDelegate, (handle, onEntity, args), default);
			}
		}
	}
}