using GameHost.Applications;
using GameHost.Core;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;

namespace PataNext.Module.RhythmEngine
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	[UpdateAfter(typeof(InitializeSystem))]
	public abstract class RhythmEngineSystemBase : AppSystem
	{
		protected RhythmEngineSystemBase(WorldCollection collection) : base(collection)
		{
		}
	}
}