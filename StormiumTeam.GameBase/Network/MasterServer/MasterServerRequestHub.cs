using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using MagicOnion;
using STMasterServer.Shared.Services;
using StormiumTeam.GameBase.Network.MasterServer.User;
using StormiumTeam.GameBase.Network.MasterServer.Utility;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public abstract class MasterServerRequestHub<THub, TReceiver, TRequestComponent> : MasterServerRequestServiceMarkerDefaultEcs<TRequestComponent>
		where THub : class, IStreamingHub<THub, TReceiver>
	{
		public THub? Service { get; protected set; }
		
		private HubClientConnectionCache<THub, TReceiver> client;
		private CurrentUserSystem                         currentUserSystem;

		protected UserToken CurrentToken { get; private set; }

		protected MasterServerRequestHub([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref client);
			DependencyResolver.Add(() => ref currentUserSystem);
		}

		protected override void OnFeatureAdded(Entity entity, MasterServerFeature obj)
		{
			client.Client.ContinueWith(task =>
			{
				if (task.Result is null)
					throw new InvalidOperationException($"The Hub <{typeof(THub).Name}, {typeof(TReceiver).Name}> shouldn't be null.");
				Service = task.Result;
				
				if (false == currentUserSystem.User.Equals(default))
					OnUserChange(currentUserSystem.User);
			});
			
			AddDisposable(World.Mgr.SubscribeComponentChanged((in Entity e, in CurrentUser prev, in CurrentUser curr) =>
			{
				OnUserChange(curr.Value);
			}));
		}

		private void OnUserChange(UserToken token)
		{
			CurrentToken = token;

			if (!string.IsNullOrEmpty(token.Representation) && Service is IServiceSupplyUserToken)
			{
				var method = Service.GetType()
				                    .GetMethod("SupplyUserToken");

				method.Invoke(Service, new object?[] {token});
			}
		}

		protected override void OnFeatureRemoved(Entity entity, MasterServerFeature obj)
		{
			base.OnFeatureRemoved(entity, obj);

			Service = null!;
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate() && Service is not null;
		}
	}
}