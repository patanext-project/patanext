using System;
using GameHost.Core.Ecs;
using StormiumTeam.GameBase;

namespace PataNext.CoreMissions.Mixed.Missions
{
	public class DebugMissionRegister : RegisterMissionSystemBase
	{
		public DebugMissionRegister(WorldCollection collection) : base(collection)
		{
		}

		public static readonly ResPath MissionPath = Utility.GetMissionPath("debug");
		public static readonly ResPath ScenarPath  = Utility.GetScenarPath("debug");

		protected override void Register()
		{
			Console.WriteLine("Register Debug Mission");
			MissionManager.Register(MissionPath, "Debug", ScenarPath);
		}
	}
}