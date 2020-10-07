using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Utility;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace StormiumTeam.GameBase.SystemBase
{
	public abstract class GameModeSystemBase<TGameMode> : GameAppSystem
		where TGameMode : struct, IComponentData
	{
		protected ILogger Logger;

		protected TaskScheduler TaskScheduler { get; private set; }

		public GameModeSystemBase(WorldCollection collection, TaskScheduler taskScheduler) : base(collection)
		{
			TaskScheduler = taskScheduler;

			DependencyResolver.Add(() => ref Logger);
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
					ccs = new CancellationTokenSource();
					currentTask = Task.Factory
					                  .StartNew(() => GetStateMachine(ccs.Token), ccs.Token, TaskCreationOptions.AttachedToParent, TaskScheduler)
					                  .Unwrap();
					break;
				case false when currentTask != null:
					ccs?.Dispose();

					ccs         = null;
					currentTask = null;
					break;
			}

			if (currentTask != null && currentTask.IsFaulted)
			{
				Logger.ZLogError(currentTask.Exception, "GameMode Exception!");
				ccs?.Dispose();

				currentTask = null;
				ccs         = null;

				// What should we do if the GameMode crash?
				// Removing the entity does not seems like an ideal solution...
				// Maybe adding a component like 'HasCrashed' would work better?
				gameModeQuery.RemoveAllEntities();
			}

			if (TaskScheduler is SameThreadTaskScheduler sameThreadTaskScheduler)
				sameThreadTaskScheduler.Execute();
		}

		public override void Dispose()
		{
			base.Dispose();

			ccs?.Dispose();
		}

		public void Do(Action ac) => ac();
	}
}