using GameHost.Applications;
using GameHost.Core;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;

namespace PataNext.Module.Simulation.RhythmEngine
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	[UpdateAfter(typeof(RhythmEngineInitializeSystem))]
	public abstract class RhythmEngineSystemBase : AppSystem
	{
		protected RhythmEngineSystemBase(WorldCollection collection) : base(collection)
		{
		}
	}
}