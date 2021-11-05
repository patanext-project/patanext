using System;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GameModes.City;

namespace PataNext.Module.Simulation.GameModes.InBasement
{
	public partial class AtCityGameModeSystem
	{
		private void onLaunchCoopMission(in LaunchCoopMissionMessage message)
		{
			if (!GameWorld.TryGetSingleton<AtCityGameModeData>(out GameEntityHandle handle))
				return;

			if (!missionManager.TryGet(message.Mission, out var missionEntity))
				throw new InvalidOperationException("No mission found with path " + message.Mission.FullString);

			var newGameMode = GameWorld.CreateEntity();
			AddComponent(newGameMode, new CoopMission());
			AddComponent(newGameMode, new CoopMission.TargetMission { Entity = missionEntity });

			GameWorld.UpdateOwnedComponent(handle, new CityCurrentGameModeTarget(Safe(newGameMode)));
		}
	}
}