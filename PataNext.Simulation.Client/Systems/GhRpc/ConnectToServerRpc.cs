using ENet;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Transports;
using Newtonsoft.Json;
using PataNext.Simulation.Client;

namespace PataNext.Module.Simulation.Systems.GhRpc
{
	public class ConnectToServerRpc : RpcCommandSystem
	{
		public ConnectToServerRpc(WorldCollection collection) : base(collection)
		{
		}

		private struct Request
		{
			public string Host;
			public int    Port;
		}

		public override string CommandId => "connect_to_server";
		
		protected override void   OnReceiveRequest(GameHostCommandResponse response)
		{
			using (var set = World.Mgr.GetEntities()
			                      .With<IListener>()
			                      .With<IClientSimulationApplication>()
			                      .AsSet())
			{
				if (set.Count == 0)
				{
					GetReplyWriter().WriteStaticString(JsonConvert.SerializeObject(new
					{
						IsError = 0,
						Message = "Couldn't find a client application"
					}));
					return;
				}

				var request = JsonConvert.DeserializeObject<Request>(response.Data.ReadString());

				var driver  = new ENetTransportDriver(1);
				var address = new Address();
				address.SetHost(request.Host);
				address.Port = (ushort) request.Port;

				driver.Connect(address);
				var reliableChannel = driver.CreateChannel(typeof(ReliableChannel));

				foreach (var entity in set.GetEntities())
				{
					var app = entity.Get<IListener>() as SimulationApplication;
					app.Data.World.CreateEntity()
					   .Set<IFeature>(new ClientFeature(driver, reliableChannel));
				}
			}
		}

		protected override void   OnReceiveReply(GameHostCommandResponse   response)
		{
			
		}
	}
}