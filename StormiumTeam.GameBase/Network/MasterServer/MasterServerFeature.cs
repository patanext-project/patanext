using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Applications;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public class MasterServerFeature : IFeature
	{
		public readonly GrpcChannel Channel;

		public MasterServerFeature(GrpcChannel channel)
		{
			this.Channel = channel;
		}

		public MasterServerFeature(string address)
		{
			Channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
			{
				Credentials = ChannelCredentials.Insecure,
#if NET
				LoggerFactory = LoggerFactory.Create(builder => builder.AddZLoggerConsole()),
				HttpHandler = new SocketsHttpHandler()
				{
					EnableMultipleHttp2Connections = true,
				}
#endif
			});
		}
	}
}