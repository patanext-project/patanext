using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public struct GameComboState : IComponentData
	{
		public class Register : RegisterGameHostComponentData<GameComboState>
		{
		}
	}
}