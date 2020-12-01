using GameHost.IO;

namespace StormiumTeam.GameBase.Arena
{
	public class ArenaResource : Resource
	{

	}

	public readonly struct ArenaResourceId
	{
		public readonly string Value;

		public ArenaResourceId(string value)
		{
			Value = value;
		}
	}
}