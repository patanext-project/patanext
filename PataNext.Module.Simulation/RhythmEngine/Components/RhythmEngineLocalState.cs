using System;

namespace PataNext.Module.Simulation.RhythmEngine
{
	public interface IRhythmEngineState
	{
		public bool     IsNewBeat { get; set; }
		public TimeSpan Elapsed   { get; set; }
	}

	public struct RhythmEngineLocalState : IRhythmEngineState
	{
		public bool     IsNewBeat { get; set; }
		public TimeSpan Elapsed   { get; set; }

		public bool CanRunCommands => Elapsed > TimeSpan.Zero;
	}
}