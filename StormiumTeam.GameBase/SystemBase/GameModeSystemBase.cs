using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using GameHost.Utility;
using Microsoft.Extensions.Logging;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Network.Authorities;
using ZLogger;

namespace StormiumTeam.GameBase.SystemBase
{
	[UpdateBefore(typeof(SendSnapshotSystem))]
	public abstract class GameModeSystemBase<TGameMode> : GameAppSystem, IPostUpdateSimulationPass
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

		protected EntityQuery GameModeQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			GameModeQuery = CreateEntityQuery(new[] {typeof(TGameMode)});
		}

		protected abstract Task GetStateMachine(CancellationToken token);

		private Task                    currentTask;
		private CancellationTokenSource ccs;

		public void OnAfterSimulationUpdate()
		{
			base.OnUpdate();

			switch (GameModeQuery.Any())
			{
				case true when currentTask == null:
					ccs         = new CancellationTokenSource();
					currentTask = TaskRunUtility.StartUnwrap(GetStateMachine, TaskScheduler, ccs.Token);
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
				GameModeQuery.RemoveAllEntities();
			}

			if (TaskScheduler is SameThreadTaskScheduler sameThreadTaskScheduler)
				sameThreadTaskScheduler.Execute();
		}

		public override void Dispose()
		{
			base.Dispose();

			ccs?.Dispose();
		}

		public GameEntityHandle GetGameModeHandle()
		{
			foreach (var handle in GameModeQuery)
				return handle;

			return default;
		}

		public void Do(Action ac) => ac();

		/// <summary>
		/// Force an action to be done with an authority on an entity.
		/// </summary>
		/// <param name="entity">The entity to be authoritative on</param>
		/// <param name="ac">The action to do when we have authority</param>
		/// <typeparam name="TAuthority">Authority type</typeparam>
		/// <returns>A task that can be used to track when the authority has been given back</returns>
		public Task RequestWithAuthority<TAuthority>(GameEntity entity, Action ac, uint additionalFrames = 0)
			where TAuthority : struct, IEntityComponent
		{
			GameWorld.AddComponent(entity.Handle, new ForceTemporaryAuthority<TAuthority>());
			if (additionalFrames > 0 && GameWorld.TryGetSingleton(out GameTime gameTime))
			{
				GetComponentData<ForceTemporaryAuthority<TAuthority>>(entity)
					.SetFrame = (int) (gameTime.Frame + 1 + additionalFrames);
			}
			
			ac();
			return Task.CompletedTask;
		}
	}
}