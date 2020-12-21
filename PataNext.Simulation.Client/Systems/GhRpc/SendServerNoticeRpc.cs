using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.Application;
using GameHost.Threading;
using PataNext.Simulation.Client;

namespace PataNext.Module.Simulation.Systems.GhRpc
{
	public class SendServerNoticeRpc : RpcCommandSystem
	{
		public SendServerNoticeRpc(WorldCollection collection) : base(collection)
		{
		}

		public override string CommandId => "notice";
		protected override void   OnReceiveRequest(GameHostCommandResponse response)
		{
			using (var set = World.Mgr.GetEntities()
			                      .With<IListener>()
			                      .With<IClientSimulationApplication>()
			                      .AsSet())
			{
				var isConnected = false;
				foreach (var appEntity in set.GetEntities())
				{
					var app = appEntity.Get<IListener>() as SimulationApplication;
					foreach (var feature in app.Data.World.Get<IFeature>())
					{
						if (feature is ClientFeature)
							isConnected = true;
					}
				}
				
				var reply = GetReplyWriter();
				reply.WriteValue(isConnected);
			}
		}

		protected override void   OnReceiveReply(GameHostCommandResponse   response)
		{
			throw new System.NotImplementedException();
		}
	}
}