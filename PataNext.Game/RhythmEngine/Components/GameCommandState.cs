using System;

namespace PataponGameHost.RhythmEngine.Components
{
	public struct GameCommandState
	{
		public int StartTimeMs;
		public int EndTimeMs;
		public int ChainEndTimeMs;

		public void Reset()
		{
			StartTimeMs = EndTimeMs = ChainEndTimeMs = -1;
		}
	}
}