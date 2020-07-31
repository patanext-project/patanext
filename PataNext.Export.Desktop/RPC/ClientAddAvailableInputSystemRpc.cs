using System;
using System.Collections.Generic;
using System.Net;
using DefaultEcs;
using ENet;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Transports;
using GameHost.Worlds;
using Newtonsoft.Json;
using PataNext.Export.Desktop;

namespace GameHost.Core.RPC.AvailableRpcCommands
{
	public class ClientAddAvailableInputSystemRpc : RpcCommandSystem
	{
		private GlobalWorld globalWorld;
		
		public ClientAddAvailableInputSystemRpc(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref globalWorld);
		}

		public override string CommandId => "addinputsystem";

		protected override void OnReceiveRequest(GameHostCommandResponse response)
		{
			Console.WriteLine("received input system!");
			var conType = response.Data.ReadString();
			var conAddr = response.Data.ReadString();
			if (conType != "enet")
				return;

			var ep = IPEndPoint.Parse(conAddr);
			var addr = new Address();
			addr.SetIP("127.0.0.1");
			addr.Port = (ushort) ep.Port;
			
			var ent = globalWorld.World.CreateEntity();
			ent.Set<TransportAddress>(new ENetTransportAddress(addr));
			ent.Set<ConnectionToInput>();
			
			Console.WriteLine($"BackendAddr={ep.ToString()}");
		}

		protected override void OnReceiveReply(GameHostCommandResponse response)
		{
			// what
		}
	}
}