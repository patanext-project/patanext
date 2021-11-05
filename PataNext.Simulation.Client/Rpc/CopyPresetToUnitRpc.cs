using System;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using StormiumTeam.GameBase.Network.MasterServer.Utility;

namespace PataNext.Simulation.Client.Rpc
{
	public struct CopyPresetToUnitRpc : IGameHostRpcPacket
	{
		public MasterServerUnitId       Unit;
		public MasterServerUnitPresetId Preset;

		public class Process : RpcPacketSystem<CopyPresetToUnitRpc>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.CopyPresetToUnit";

			protected override void OnNotification(CopyPresetToUnitRpc notification)
			{
				var app = GetClientAppUtility.Get(this);
				Console.WriteLine("?????????");
				app.Scheduler.Schedule(args =>
				{
					var req = args.notification;
					Console.WriteLine("!!!!!!!");
					RequestUtility.CreateFireAndForget(args.World, new CopyPresetToTargetUnitRequest(req.Preset.Value, req.Unit.Value));
				}, (app.Data.World, notification), default);
			}
		}
	}
}