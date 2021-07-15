using GameHost.Core.Ecs;
using StormiumTeam.GameBase;

namespace PataNext.CoreMissions.Mixed.Missions.City
{
	public class PatapolisCityRegister : RegisterMissionSystemBase
	{
		public static readonly ResPath MissionPath = Utility.GetMissionPath("city/patapolis");
		public static readonly ResPath ScenarPath  = Utility.GetScenarPath("scenar/city/patapolis");

		public PatapolisCityRegister(WorldCollection collection) : base(collection)
		{
		}

		protected override void Register()
		{
			MissionManager.Register(MissionPath, "Patapolis City", ScenarPath);
		}
	}
}