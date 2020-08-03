using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	/// <summary>
	///     Tag component
	/// </summary>
	public struct RhythmEngineIsPlaying : IComponentData
	{
		public class Register : RegisterGameHostComponentData<RhythmEngineIsPlaying>
		{
		}
	}
}