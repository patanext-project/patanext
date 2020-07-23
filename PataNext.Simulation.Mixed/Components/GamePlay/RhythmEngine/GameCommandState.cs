using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public struct GameCommandState : IComponentData
	{
		public int StartTimeMs;
		public int EndTimeMs;
		public int ChainEndTimeMs;

		public void Reset()
		{
			StartTimeMs = EndTimeMs = ChainEndTimeMs = -1;
		}

		public class Register : RegisterGameHostComponentData<GameCommandState>
		{
		}
	}
}