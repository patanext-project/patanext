using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public struct RhythmEnginePredictedCommandBuffer : IComponentBuffer
	{
		public GameResource<RhythmCommandResource> Value;

		public class Register : RegisterGameHostComponentBuffer<RhythmEnginePredictedCommandBuffer>
		{
		}
	}
}