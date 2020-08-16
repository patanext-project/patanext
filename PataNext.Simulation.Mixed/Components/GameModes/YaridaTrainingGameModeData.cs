using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GameModes
{
	public struct YaridaTrainingGameModeData : IComponentData
	{
		public enum EPhase
		{
			Waiting = 0,
			March   = 1,

			// huehuehuehuheuheueheuheuehuheueheuheuehuehueheueheuheuheuhue
			Backward = 2
		}
		
		public EPhase Phase;
		public float CurrUberHeroPos;
		public int    YaridaOvertakeCount;

		public float LastCheckpointScore;
		public float LastCheckpointTime;
		
		public class Register : RegisterGameHostComponentData<YaridaTrainingGameModeData>
		{}
	}
}