using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Revolution.NetCode.Components;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Time
{
	public class TimeFromForeignSystem : GameAppSystem, IAfterSnapshotDataPass
	{
		private IScheduler scheduler;
		
		public TimeFromForeignSystem([NotNull] WorldCollection collection) : base(collection)
		{
			scheduler = new Scheduler();
		}

		private EntityQuery withoutQuery;
		private EntityQuery query;

		private static Action<(GameWorld gameWorld, GameEntityHandle handle, ComponentType ct)> addComponent = args => args.gameWorld.AddComponent(args.handle, args.ct);
		

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			withoutQuery = CreateEntityQuery(new[]
			{
				typeof(ForeignNetTime)
			}, new[]
			{
				typeof(NetReportTime),
				typeof(SnapshotOwnedWriteArchetype),
				typeof(SnapshotEntity.CreatedByThisWorld)
			});
			
			query = CreateEntityQuery(new[]
			{
				typeof(ForeignNetTime), typeof(NetReportTime)
			}, new[]
			{
				typeof(SnapshotOwnedWriteArchetype),
				typeof(SnapshotEntity.CreatedByThisWorld)
			});
		}

		public void AfterSnapshotData()
		{
			foreach (var entity in withoutQuery)
			{
				scheduler.Schedule(addComponent, (GameWorld, entity, AsComponentType<NetReportTime>()), default);
			}

			scheduler.Run();

			var foreignAccessor = GetAccessor<ForeignNetTime>();
			var localAccessor   = GetAccessor<NetReportTime>();

			foreach (var entity in query)
			{
				ref readonly var foreign = ref foreignAccessor[entity];
				ref var          report  = ref localAccessor[entity];

				if (foreign.ClientTime.begin.Frame != report.Begin.Frame || foreign.ClientTime.end.Frame != report.End.Frame)
					report.Continuous = 0;
				else
					report.Continuous++;

				report.Begin = foreign.ClientTime.begin;
				report.End   = foreign.ClientTime.end;

				//Console.WriteLine($"YEP update");
			}
		}
	}
}