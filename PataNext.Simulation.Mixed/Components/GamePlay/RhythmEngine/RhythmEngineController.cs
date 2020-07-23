using System;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public enum RhythmEngineState
	{
		Stopped = 0,
		Paused  = 1,
		Playing = 2
	}

	public struct RhythmEngineController : IComponentData
	{
		public RhythmEngineState State;
		public TimeSpan          StartTime;

		public class Register : RegisterGameHostComponentData<RhythmEngineController>
		{
		}
	}
}