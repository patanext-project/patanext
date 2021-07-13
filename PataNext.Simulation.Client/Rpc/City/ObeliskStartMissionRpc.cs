using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components.GameModes.City;
using StormiumTeam.GameBase;

namespace PataNext.Simulation.Client.Rpc.City
{
	public struct ObeliskStartMissionRpc : IGameHostRpcPacket
	{
		public string Path;

		public class Process : RpcPacketSystem<ObeliskStartMissionRpc>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.Client.City.ObeliskStartMission";

			protected override void OnNotification(ObeliskStartMissionRpc notification)
			{
				var app = GetClientAppUtility.Get(this);
				app.Schedule(() => { app.Data.World.Publish(new LaunchCoopMissionMessage(new(notification.Path))); }, default);
			}
		}
	}
}