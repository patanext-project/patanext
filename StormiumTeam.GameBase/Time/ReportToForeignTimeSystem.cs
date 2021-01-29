using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.Components;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Time
{
	public class ReportToForeignTimeSystem : GameAppSystem, IAfterSnapshotDataPass
	{
		public ReportToForeignTimeSystem([NotNull] WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery query;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			query = CreateEntityQuery(new[] {typeof(ForeignNetTime), typeof(NetReportTime)});
		}

		public void AfterSnapshotData()
		{
			var foreignAccessor = GetAccessor<ForeignNetTime>();
			var localAccessor   = GetAccessor<NetReportTime>();
			foreach (var entity in query)
			{
				var local = localAccessor[entity];
				foreignAccessor[entity] = new ForeignNetTime
				{
					ClientTime = (local.Begin, local.End)
				};
			}
		}
	}
}