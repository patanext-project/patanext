using System.Diagnostics;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.Application;
using GameHost.Threading;
using PataNext.Simulation.Client;

namespace PataNext.Module.Simulation.Systems.GhRpc
{
	public struct DisconnectFromServerRpc : IGameHostRpcPacket
	{

		public class System : RpcPacketSystem<DisconnectFromServerRpc>
		{
			public System(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.Tests.LeaveServer";

			protected override void OnNotification(DisconnectFromServerRpc notification)
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
						using var subset = app.Data.World.GetEntities()
						                      .With<IFeature>()
						                      .AsSet();

						foreach (var featureEnt in subset.GetEntities())
						{
							if (featureEnt.Get<IFeature>() is not ClientFeature clientFeature) 
								continue;
							
							clientFeature.Driver.Dispose();
							featureEnt.Dispose();
							break;
						}
					}, default);
				}
			}
		}
	}
}