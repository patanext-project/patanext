using System;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using GameHost.Utility;
using MagicOnion;
using MagicOnion.Client;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public abstract class MasterServerRequestServiceMarkerBase<TService, TEntity, TRequestComponent> : AppSystemWithFeature<MasterServerFeature>
		where TService : class, IServiceMarker
	{
		protected TaskScheduler TaskScheduler = null!;

		public MasterServerRequestServiceMarkerBase(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref TaskScheduler);
		}

		public TService? Service { get; protected set; }

		protected virtual bool ManageCallerStatus => false;

		protected abstract Task OnUnprocessedRequest(TEntity entity, RequestCallerStatus callerStatus);
	}

	public abstract class MasterServerRequestServiceMarkerDefaultEcs<TService, TRequestComponent> : MasterServerRequestServiceMarkerBase<TService, Entity, TRequestComponent>
		where TService : class, IServiceMarker
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
			if (Service is null)
				return;

			foreach (var entity in unprocessedEntitySet.GetEntities())
			{
				TaskScheduler.StartUnwrap(() => ProcessRequest(entity));
			}
			
			unprocessedEntitySet.Set<InProcess<TRequestComponent>>();
		}

		protected virtual void FillRule(ref EntityRuleBuilder rule)
		{
			rule.With<TRequestComponent>()
			    .Without<InProcess<TRequestComponent>>();
		}

		private async Task ProcessRequest(Entity entity)
		{
			var type = RequestCallerStatus.DestroyCaller;
			if (!entity.Has<UntrackedRequest>())
				type = RequestCallerStatus.KeepCaller;

			await OnUnprocessedRequest(entity, type);
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