using System;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public struct RhythmEngineSettings : IComponentData
	{
		public TimeSpan BeatInterval;
		public int      MaxBeat;

		public class Register : RegisterGameHostComponentData<RhythmEngineSettings>
		{
		}
	}
}