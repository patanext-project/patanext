using System;
using RevolutionSnapshot.Core.ECS;

namespace PataNext.Module.RhythmEngine
{
	public enum EngineControllerState
	{
		Stopped = 0,
		Paused  = 1,
		Playing = 2
	}

	public struct RhythmEngineController
	{
		public EngineControllerState State;
		public TimeSpan              StartTime;
	}
}