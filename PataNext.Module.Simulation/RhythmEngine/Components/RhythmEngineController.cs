using System;
using RevolutionSnapshot.Core.ECS;

namespace PataNext.Module.Simulation.RhythmEngine
{
	public enum RhythmEngineState
	{
		Stopped = 0,
		Paused  = 1,
		Playing = 2
	}

	public struct RhythmEngineController : IRevolutionComponent
	{
		public RhythmEngineState State;
		public TimeSpan          StartTime;
	}
}