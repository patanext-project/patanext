using RevolutionSnapshot.Core.ECS;

namespace PataNext.Module.Simulation.RhythmEngine
{
	public struct RhythmEngineOnNewBeat : IRevolutionComponent
	{
		public int Previous, Next;
	}
}