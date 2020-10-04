using System;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using MagicOnion;
using MagicOnion.Client;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public enum RequestCallerStatus
	{
		/// <summary>
		/// This will destroy the caller entity
		/// </summary>
		DestroyCaller = 0,

		/// <summary>
		/// This will keep the caller entity alive.
		/// </summary>
		KeepCaller = 1
	}

	public struct InProcess<T> : IComponentData
	{
	}

	public struct UntrackedRequest : IComponentData
	{

	}

	public abstract class MasterServerRequestService<TService, TRequestComponent> : AppSystemWithFeature<MasterServerFeature>
		where TService : class, IService<TService>
	{
		private TaskScheduler taskScheduler;
		private EntitySet     unprocessedEntitySet;

		public MasterServerRequestService(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref taskScheduler);
		}

		protected override void OnInit()
		{
			base.OnInit();

			var ruleBuilder = World.Mgr.GetEntities();
			FillRule(ref ruleBuilder);

			unprocessedEntitySet = ruleBuilder.AsSet();
		}
		
		public TService Service { get; private set; }

		protected virtual bool ManageCallerStatus => false;

		protected override void OnFeatureAdded(MasterServerFeature obj)
		{
			Service = MagicOnionClient.Create<TService>(obj.Channel);
		}

		protected override void OnFeatureRemoved(MasterServerFeature obj)
		{
			Service = null;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (Service == null)
				return;
			
			foreach (var entity in unprocessedEntitySet.GetEntities())
			{
				entity.Set(new InProcess<TRequestComponent>());
				Task.Factory.StartNew(() => ProcessRequest(entity), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
			}
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
			if (ManageCallerStatus && type == RequestCallerStatus.DestroyCaller)
				entity.Dispose();
			else
			{
				entity.Remove<TRequestComponent>();
				entity.Remove<InProcess<TRequestComponent>>();
			}
		}

		protected abstract Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus);
	}
}