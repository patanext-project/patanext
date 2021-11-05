using StormiumTeam.GameBase;

namespace PataNext.CoreMissions.Mixed
{
	public static class Utility
	{
		public static ResPath GetMissionPath(string name)
		{
			return new(ResPath.EType.MasterServer, "st", "pn", new[] {"mission", name});
		}
		
		public static ResPath GetScenarPath(string name)
		{
			return new(ResPath.EType.MasterServer, "st", "pn", new[] {"mission", "scenar", name});
		}
	}
}