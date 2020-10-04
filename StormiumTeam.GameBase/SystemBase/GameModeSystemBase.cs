using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Utility;

namespace StormiumTeam.GameBase.SystemBase
{
	public abstract class GameModeSystemBase<TGameMode> : GameAppSystem
		where TGameMode : struct, IComponentData
	{
		protected TaskScheduler TaskScheduler { get; private set; }

		public GameModeSystemBase(WorldCollection collection, TaskScheduler taskScheduler) : base(collection)
		{
			TaskScheduler = taskScheduler;
		}

		public GameModeSystemBase(WorldCollection collection) : this(collection, new SameThreadTaskScheduler())
		{
		}

		private EntityQuery gameModeQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			gameModeQuery = CreateEntityQuery(new[] {typeof(TGameMode)});
		}

		protected abstract Task GetStateMachine(CancellationToken token);

		private Task                    currentTask;
		private CancellationTokenSource ccs;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			switch (gameModeQuery.Any())
			{
				case true when currentTask == null:
					ccs         = new CancellationTokenSource();
					currentTask = Task.Factory.StartNew(() => GetStateMachine(ccs.Token), ccs.Token, TaskCreationOptions.AttachedToParent, TaskScheduler);
					break;
				case false when currentTask != null:
					ccs.Cancel();

					ccs         = null;
					currentTask = null;
					break;
			}

			if (TaskScheduler is SameThreadTaskScheduler sameThreadTaskScheduler)
				sameThreadTaskScheduler.Execute();
		}

		public override void Dispose()
		{
			base.Dispose();

			ccs?.Dispose();
		}
	}
}