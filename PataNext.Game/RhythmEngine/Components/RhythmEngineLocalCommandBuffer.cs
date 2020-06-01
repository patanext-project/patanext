using System;
using System.Collections.Generic;
using GameHost.HostSerialization;
using PataNext.Module.RhythmEngine.Data;

namespace PataNext.Module.RhythmEngine
{
	// todo: we should make it as a struct
	public class RhythmEngineLocalCommandBuffer : List<FlowPressure>, ICopyable<RhythmEngineLocalCommandBuffer>
	{
		public void CopyTo(ref RhythmEngineLocalCommandBuffer other)
		{
			other.Clear();
			other.AddRange(this);
		}
	}
}