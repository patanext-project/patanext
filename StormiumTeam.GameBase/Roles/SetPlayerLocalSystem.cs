using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Roles
{
	public class SetPlayerLocalSystem : GameAppSystem
	{
		public SetPlayerLocalSystem([NotNull] WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery query;
		private IScheduler  post;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			query = CreateEntityQuery(new[]
			{
				typeof(PlayerDescription),
				typeof(SnapshotOwnedWriteArchetype)
			}, new []
			{
				typeof(PlayerIsLocal)
			});

			addLocal = handle => AddComponent<PlayerIsLocal>(handle);
			post     = new Scheduler();
		}

		protected override void OnUpdate()
		{
			foreach (var handle in query)
			{
				post.Schedule(addLocal, handle, default);
			}
			post.Run();
		}

		private Action<GameEntityHandle> addLocal;
	}
}