using System.Collections.Generic;
using GameHost.Applications;
using Grpc.Core;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public class MasterServerFeature : IFeature
	{
		public readonly Channel Channel;

		public MasterServerFeature(Channel channel)
		{
			this.Channel = channel;
		}

		public MasterServerFeature(string host, int port)
		{
			Channel = new Channel(host, port, ChannelCredentials.Insecure);
		}
	}
}