using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Simulation.TabEcs;
using Microsoft.Extensions.Logging;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components.GameModes.City;
using ZLogger;

namespace PataNext.Simulation.Client.Rpc.City
{
	public struct ModifyPlayerCityLocationRpc : IGameHostRpcPacket
	{
		public GameEntity LocationEntity;

		public class Process : RpcPacketSystem<ModifyPlayerCityLocationRpc>
		{
			private ILogger logger;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref logger);
			}

			public override string MethodName => "PataNext.Client.City.ModifyPlayerCityLocation";

			protected override void OnNotification(ModifyPlayerCityLocationRpc notification)
			{
				var app = GetClientAppUtility.Get(this);
				app.Schedule(() =>
				{
					var player = GetClientAppUtility.GetLocalPlayer(app);
					if (player.Has<PlayerCurrentCityLocation>())
					{
						if (notification.LocationEntity != default && !player.GameWorld.Exists(notification.LocationEntity))
						{
							notification.LocationEntity = default;
							logger.ZLogWarning("{0} doesn't exist", notification.LocationEntity);
						}

						player.GetData<PlayerCurrentCityLocation>().Entity = notification.LocationEntity;
					}
				}, default);
			}
		}
	}
}