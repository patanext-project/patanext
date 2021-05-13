using System;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using MagicOnion;
using MagicOnion.Client;

namespace StormiumTeam.GameBase.Network.MasterServer.Utility
{
	public sealed class HubClientConnectionCache<THub, TReceiver> : AppSystemWithFeature<MasterServerFeature>
		where THub : IStreamingHub<THub, TReceiver>
	{
		public TReceiver   Receiver { get; private set; }
		public Task<THub?> Client   { get; private set; }

		private TReceiver receiver;

		public HubClientConnectionCache(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref receiver);
		}

		protected override void OnFeatureAdded(Entity entity, MasterServerFeature obj)
		{
			base.OnFeatureAdded(entity, obj);
			
			Client = StreamingHubClient.ConnectAsync<THub, TReceiver>(obj.Channel, receiver)!;
		}

		protected override void OnFeatureRemoved(Entity entity, MasterServerFeature obj)
		{
			base.OnFeatureRemoved(entity, obj);

			Client = Task.FromResult((THub?) default);
		}
	}
}