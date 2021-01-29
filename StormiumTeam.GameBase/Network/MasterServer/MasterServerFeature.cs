using System.Collections.Generic;
using GameHost.Applications;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

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
				LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole())
			});
		}
	}
}