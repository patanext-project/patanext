using System;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public interface IRhythmEngineState
	{
		public FlowPressure LastPressure           { get; set; }
		public int          RecoveryActivationBeat { get; set; }
		public TimeSpan     Elapsed                { get; set; }

		bool CanRunCommands                 => Elapsed > TimeSpan.Zero;
		bool IsRecovery(int activationBeat) => RecoveryActivationBeat > activationBeat;
	}
}