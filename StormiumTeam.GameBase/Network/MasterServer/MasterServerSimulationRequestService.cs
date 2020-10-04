using System.Threading;
using System.Threading.Tasks;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using MagicOnion;
using MagicOnion.Client;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public abstract class MasterServerSimulationRequestService<TService, TRequestComponent> : AppSystemWithFeature<MasterServerFeature>
		where TService : class, IService<TService> where TRequestComponent : struct, IEntityComponent
	{
		protected GameWorld GameWorld;
		
		private TaskScheduler taskScheduler;
		private EntityQuery   unprocessedEntityQuery;

		public MasterServerSimulationRequestService(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref taskScheduler);
			DependencyResolver.Add(() => ref GameWorld);
		}

		protected override void OnInit()
		{
			base.OnInit();

			var all  = new PooledList<ComponentType>();
			var none = new PooledList<ComponentType>();
			FillQuery(all, none);

			unprocessedEntityQuery = new EntityQuery(GameWorld, all.Span, none.Span);
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
			
			foreach (var entity in unprocessedEntityQuery.GetEntities())
			{
				GameWorld.AddComponent(entity, new InProcess<TRequestComponent>());
				Task.Factory.StartNew(() => ProcessRequest(entity), CancellationToken.None, TaskCreationOptions.None, taskScheduler);
			}
		}

		protected virtual void FillQuery(PooledList<ComponentType> all, PooledList<ComponentType> none)
		{
			all.Add(GameWorld.AsComponentType<TRequestComponent>());
			none.Add(GameWorld.AsComponentType<InProcess<TRequestComponent>>());
		}

		private async Task ProcessRequest(GameEntity entity)
		{
			var type = RequestCallerStatus.DestroyCaller;
			if (!GameWorld.HasComponent<UntrackedRequest>(entity))
				type = RequestCallerStatus.KeepCaller;

			await OnUnprocessedRequest(entity, type);
			if (ManageCallerStatus && type == RequestCallerStatus.DestroyCaller)
				GameWorld.RemoveEntity(entity);
			else
			{
				GameWorld.RemoveComponent(entity, GameWorld.AsComponentType<InProcess<TRequestComponent>>());
				GameWorld.RemoveComponent(entity, GameWorld.AsComponentType<TRequestComponent>());
			}
		}

		protected abstract Task OnUnprocessedRequest(GameEntity entity, RequestCallerStatus callerStatus);
	}
}