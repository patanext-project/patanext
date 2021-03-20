using System.Diagnostics;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.Application;
using GameHost.Threading;
using PataNext.Simulation.Client;
using NotImplementedException = System.NotImplementedException;

namespace PataNext.Module.Simulation.Systems.GhRpc
{
	public struct SendServerNoticeRpc : IGameHostRpcWithResponsePacket<SendServerNoticeRpc.Response>
	{
		public struct Response : IGameHostRpcResponsePacket
		{
			public bool IsConnected { get; set; }
		}

		public class System : RpcPacketWithResponseSystem<SendServerNoticeRpc, Response>
		{
			public System(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.Tests.Notice";

			protected override Response GetResponse(in SendServerNoticeRpc request)
			{
				using var set = World.Mgr.GetEntities()
				                     .With<IListener>()
				                     .With<IClientSimulationApplication>()
				                     .AsSet();

				var isConnected = false;
				foreach (var appEntity in set.GetEntities())
				{
					var app = appEntity.Get<IListener>() as SimulationApplication;
					Debug.Assert(app != null, nameof(app) + " != null");

					foreach (var feature in app.Data.World.Get<IFeature>())
					{
						if (feature is ClientFeature)
							isConnected = true;
					}
				}

				return new() {IsConnected = isConnected};
			}
		}
	}
}