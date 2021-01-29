using System;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;

namespace StormiumTeam.GameBase
{
	public static class EntityQueryExtensions
	{
		private static readonly Action<(GameEntityHandle handle, Action<GameEntityHandle> action)> ForEachDeferredDelegate = args => { args.action(args.handle); };

		public static void ForEachDeferred<TScheduler>(this EntityQuery query, Action<GameEntityHandle> onEntity, TScheduler scheduler,
		                                               bool checkIfEntityExist = false)
			where TScheduler : IScheduler
		{
			foreach (var handle in query)
			{
				scheduler.Schedule(ForEachDeferredDelegate, (handle, onEntity), default);
			}
		}
	}
}