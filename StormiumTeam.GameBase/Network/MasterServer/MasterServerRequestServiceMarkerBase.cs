using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using GameHost.Utility;
using MagicOnion;
using MagicOnion.Client;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public abstract class MasterServerRequestServiceMarkerBase<TEntity, TRequestComponent> : AppSystemWithFeature<MasterServerFeature>
	{
		protected TaskScheduler TaskScheduler = null!;
		protected ILogger       Logger;

		public MasterServerRequestServiceMarkerBase(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref TaskScheduler);
			DependencyResolver.Add(() => ref Logger);
		}
		
		protected virtual bool ManageCallerStatus => false;

		protected abstract Task<Action<TEntity>> OnUnprocessedRequest(TEntity entity, RequestCallerStatus callerStatus);
	}

	public abstract class MasterServerRequestServiceMarkerDefaultEcs<TRequestComponent> : MasterServerRequestServiceMarkerBase<Entity, TRequestComponent>
	{
		private EntitySet unprocessedEntitySet = null!;

		protected MasterServerRequestServiceMarkerDefaultEcs(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnInit()
		{
			base.OnInit();

			var ruleBuilder = World.Mgr.GetEntities();
			FillRule(ref ruleBuilder);

			unprocessedEntitySet = ruleBuilder.AsSet();
		}
		
		protected override void OnUpdate()
		{
			base.OnUpdate();

			foreach (var entity in unprocessedEntitySet.GetEntities())
			{
				var e = entity;
				TaskScheduler.StartUnwrap(() => ProcessRequest(e));
			}

			unprocessedEntitySet.Set<InProcess<TRequestComponent>>();
		}

		protected virtual void FillRule(ref EntityRuleBuilder rule)
		{
			rule.With<TRequestComponent>()
			    .Without<InProcess<TRequestComponent>>();
		}

		private struct PreviousTask
		{
			public int Generation;
			public int Completed;
		}

		private async Task ProcessRequest(Entity entity)
		{
			if (false == entity.Has<PreviousTask>())
				entity.Set<PreviousTask>();

			var previousTask = entity.Get<PreviousTask>();
			var next         = new PreviousTask {Generation = previousTask.Generation + 1, Completed = previousTask.Completed};
			entity.Set(next);

			var type = RequestCallerStatus.DestroyCaller;
			if (!entity.Has<UntrackedRequest>())
				type = RequestCallerStatus.KeepCaller;

			Action<Entity> action;
			try
			{
				var task = OnUnprocessedRequest(entity, type);
				action = await task;
			}
			catch (Exception ex)
			{
				Console.WriteLine("allo");
				
				Logger.ZLogCritical(ex, "On " + entity);
				return;
			}

			if (!entity.IsAlive)
				return;
			
			if (next.Generation > entity.Get<PreviousTask>().Completed)
			{
				var current = entity.Get<PreviousTask>();
				entity.Set(new PreviousTask {Generation = current.Generation, Completed = next.Generation});

				action(entity);
			}

			if (false == ManageCallerStatus && type == RequestCallerStatus.DestroyCaller)
				entity.Dispose();
			else
			{
				entity.Remove<TRequestComponent>();
				entity.Remove<InProcess<TRequestComponent>>();
			}
		}
	}
}