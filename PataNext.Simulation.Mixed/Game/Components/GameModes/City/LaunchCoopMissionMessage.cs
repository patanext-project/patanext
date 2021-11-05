using StormiumTeam.GameBase;

namespace PataNext.Module.Simulation.Components.GameModes.City
{
	public struct LaunchCoopMissionMessage
	{
		public ResPath Mission;

		public LaunchCoopMissionMessage(ResPath mission) => Mission = mission;
	}
}