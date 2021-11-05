using System;
using GameHost.Core.Ecs;
using StormiumTeam.GameBase;

namespace PataNext.CoreMissions.Mixed.Missions
{
	public class BonedethMissionRegister : RegisterMissionSystemBase
	{
		public static readonly ResPath MissionPath = Utility.GetMissionPath("test");
		public static readonly ResPath ScenarPath  = Utility.GetScenarPath("bonedeth");

		public BonedethMissionRegister(WorldCollection collection) : base(collection)
		{
		}

		protected override void Register()
		{
			Console.WriteLine("Register Bonedeth Remains");
			MissionManager.Register(MissionPath, "Bonedeth Remains", ScenarPath);
		}
	}
}