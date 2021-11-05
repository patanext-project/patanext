using System;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public struct GameCommandState : IComponentData
	{
		public AbilitySelection Selection;
		
		public int StartTimeMs;
		public int EndTimeMs;
		public int ChainEndTimeMs;

		public TimeSpan StartTimeSpan => TimeSpan.FromMilliseconds(StartTimeMs);
		public TimeSpan EndTimeSpan => TimeSpan.FromMilliseconds(EndTimeMs);
		public TimeSpan ChainEndTimeSpan => TimeSpan.FromMilliseconds(ChainEndTimeMs);

		public void Reset()
		{
			StartTimeMs = EndTimeMs = ChainEndTimeMs = -1;
		}
		
		public readonly bool IsGamePlayActive(int milliseconds)
		{
			return milliseconds >= StartTimeMs && milliseconds <= EndTimeMs;
		}

		public readonly bool IsInputActive(int milliseconds, int beatInterval)
		{
			return milliseconds >= EndTimeMs - beatInterval && milliseconds <= EndTimeMs + beatInterval;
		}

		public readonly bool HasActivity(int milliseconds, int beatInterval)
		{
			return IsGamePlayActive(milliseconds)
			       || IsInputActive(milliseconds, beatInterval);
		}

		public readonly bool HasActivity<TState>(TState state, RhythmEngineSettings settings)
			where TState : IRhythmEngineState
		{
			return HasActivity((int) (state.Elapsed.Ticks / TimeSpan.TicksPerMillisecond), (int) (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
		}

		public class Register : RegisterGameHostComponentData<GameCommandState>
		{
		}
	}
}