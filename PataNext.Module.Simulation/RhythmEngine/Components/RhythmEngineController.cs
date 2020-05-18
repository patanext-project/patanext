﻿using System;

namespace PataNext.Module.Simulation.RhythmEngine
{
	public enum RhythmEngineState
	{
		Stopped = 0,
		Paused  = 1,
		Playing = 2
	}

	public struct RhythmEngineController
	{
		public RhythmEngineState State;
		public TimeSpan          StartTime;
	}
}