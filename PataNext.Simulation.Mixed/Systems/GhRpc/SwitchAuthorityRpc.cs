using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Simulation.Application;
using GameHost.Threading;
using PataNext.Module.Simulation.GameModes.InBasement;

namespace PataNext.Module.Simulation.Systems.GhRpc
{
	public struct SwitchAuthorityRpc : IGameHostRpcPacket
	{
		public string AuthorityType { get; set; }

		public class System : RpcPacketSystem<SwitchAuthorityRpc>
		{
			public System(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.Simulation.Tests.SwitchAuthority";

			protected override void OnNotification(SwitchAuthorityRpc notification)
			{
				var request = string.IsNullOrEmpty(notification.AuthorityType) ? notification.AuthorityType : "client";
				foreach (var listener in World.Mgr.Get<IListener>())
				{
					if (listener is SimulationApplication simulationApplication)
					{
						simulationApplication.Schedule(() =>
						{
							if (simulationApplication.Data.Collection.TryGet(out AtCityGameModeSystem gameModeSystem))
							{
								gameModeSystem.SwitchAuthority(request);
							}
						}, default);
					}
				}
			}
		}
	}
}