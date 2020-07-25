using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;

namespace PataNext.Module.Simulation.Game.RhythmEngine
{
	public readonly struct RhythmCommandActionBuffer : IComponentBuffer
	{
		public readonly RhythmCommandAction Value;

		public RhythmCommandActionBuffer(RhythmCommandAction value)
		{
			Value = value;
		}
		
		public class Register : RegisterGameHostComponentBuffer<RhythmCommandActionBuffer>
		{}
	}
}