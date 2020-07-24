using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public struct RhythmEnginePredictedCommandBuffer : IComponentBuffer
	{
		public FlowPressure Value;

		public class Register : RegisterGameHostComponentBuffer<RhythmEnginePredictedCommandBuffer>
		{
		}
	}
}