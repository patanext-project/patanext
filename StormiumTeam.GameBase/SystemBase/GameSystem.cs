using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;

namespace StormiumTeam.GameBase.SystemBase
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class GameSystem : AppSystem
	{
		private GameWorld gameWorldRef;

		public GameSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref gameWorldRef);
		}

		protected GameWorld GameWorld => gameWorldRef;
	}
}