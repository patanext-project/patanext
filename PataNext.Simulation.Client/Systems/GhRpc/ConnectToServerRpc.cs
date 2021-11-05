using System.Diagnostics;
using System.Net;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.RPC;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Transports.Transports.Ruffles;
using PataNext.Simulation.Client;

namespace PataNext.Module.Simulation.Systems.GhRpc
{
	public struct ConnectToServerRpc : IGameHostRpcPacket
	{
		public string Host { get; set; }
		public int    Port { get; set; }

		public class System : RpcPacketSystem<ConnectToServerRpc>
		{
			public System(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.Tests.JoinServer";

			protected override void OnNotification(ConnectToServerRpc notification)
			{
				using var set = World.Mgr.GetEntities()
				                     .With<IListener>()
				                     .With<IClientSimulationApplication>()
				                     .AsSet();
				foreach (var entity in set.GetEntities())
				{
					var app = entity.Get<IListener>() as SimulationApplication;
					Debug.Assert(app != null, nameof(app) + " != null");

					app.Schedule(() =>
					{
						var driver  = new RuffleTransportDriver();
						var address = new IPEndPoint(IPAddress.Parse(notification.Host), notification.Port);
						driver.Connect(address);

						var reliableChannel = default(TransportChannel);

						app.Data.World.CreateEntity()
						   .Set<IFeature>(new ClientFeature(driver, reliableChannel));
					}, default);
				}
			}
		}
	}
}