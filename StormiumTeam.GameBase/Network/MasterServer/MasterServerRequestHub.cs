using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Server;
using NetFabric.Hyperlinq;
using StormiumTeam.GameBase.Network.MasterServer.Utility;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public abstract class MasterServerRequestHub<THub, TReceiver, TRequestComponent> : MasterServerRequestServiceMarkerDefaultEcs<TRequestComponent>
		where THub : class, IStreamingHub<THub, TReceiver>
	{
		public THub? Service { get; protected set; }
		
		private HubClientConnectionCache<THub, TReceiver> client;

		protected MasterServerRequestHub([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref client);
		}

		protected override void OnFeatureAdded(Entity entity, MasterServerFeature obj)
		{
			client.Client.ContinueWith(task =>
			{
				if (task.Result is null)
					throw new InvalidOperationException($"The Hub <{typeof(THub).Name}, {typeof(TReceiver).Name}> shouldn't be null.");
				Service = task.Result;
			});
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