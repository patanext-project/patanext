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
	public class DisconnectFromServerRpc : RpcCommandSystem
	{
		public DisconnectFromServerRpc(WorldCollection collection) : base(collection)
		{
		}

		private struct Request
		{
			public string Host;
			public int    Port;
		}

		public override string CommandId => "disconnect_from_server";
		
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
				
				foreach (var entity in set.GetEntities())
				{
					var app = entity.Get<IListener>() as SimulationApplication;
					app.Schedule(() =>
					{
						using var subset = app.Data.World.GetEntities()
						                      .With<IFeature>()
						                      .AsSet();

						foreach (var featureEnt in subset.GetEntities())
						{
							if (featureEnt.Get<IFeature>() is ClientFeature clientFeature)
							{
								clientFeature.Driver.Dispose();
								featureEnt.Dispose();
								break;
							}
						}
					}, default);
				}
			}
		}

		protected override void   OnReceiveReply(GameHostCommandResponse   response)
		{
			
		}
	}
}