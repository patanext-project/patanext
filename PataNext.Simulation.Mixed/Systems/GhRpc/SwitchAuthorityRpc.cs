using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Simulation.Application;
using GameHost.Threading;
using PataNext.Module.Simulation.GameModes.InBasement;

namespace PataNext.Module.Simulation.Systems.GhRpc
{
	public class SwitchAuthorityRpc : RpcCommandSystem
	{
		public SwitchAuthorityRpc(WorldCollection collection) : base(collection)
		{
		}

		public override string CommandId => "authority";
		protected override void   OnReceiveRequest(GameHostCommandResponse response)
		{
			var request = response.Data.Length > 0 ? response.Data.ReadString() : "client";
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

		protected override void   OnReceiveReply(GameHostCommandResponse   response)
		{
			
		}
	}
}