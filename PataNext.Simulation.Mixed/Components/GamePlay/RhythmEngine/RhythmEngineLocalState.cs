using System;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public struct RhythmEngineLocalState : IRhythmEngineState, IComponentData
	{
		public FlowPressure LastPressure           { get; set; }
		public int          RecoveryActivationBeat { get; set; }
		public TimeSpan     Elapsed                { get; set; }

		public int  CurrentBeat;
		public uint NewBeatTick;

		public bool CanRunCommands => Elapsed > TimeSpan.Zero;

		public readonly bool IsRecovery(int activationBeat)
		{
			return RecoveryActivationBeat > activationBeat;
		}

		public class Register : RegisterGameHostComponentData<RhythmEngineLocalState>
		{
		}
	}
}