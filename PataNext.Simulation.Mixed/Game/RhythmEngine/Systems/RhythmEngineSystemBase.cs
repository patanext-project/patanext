using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using StormiumTeam.GameBase.Time;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[RestrictToApplication(typeof(SimulationApplication))]
	[UpdateAfter(typeof(SetGameTimeSystem))]
	public abstract class RhythmEngineSystemBase : AppSystem
	{
		protected GameWorld GameWorld;

		protected RhythmEngineSystemBase(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref GameWorld);
		}
	}
}