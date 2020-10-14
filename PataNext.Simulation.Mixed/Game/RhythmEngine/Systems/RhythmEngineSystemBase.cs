using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Time;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[RestrictToApplication(typeof(SimulationApplication))]
	[UpdateAfter(typeof(SetGameTimeSystem))]
	public abstract class RhythmEngineSystemBase : GameAppSystem, IRhythmEngineSimulationPass
	{
		protected RhythmEngineSystemBase(WorldCollection collection) : base(collection)
		{
		}

		public abstract void OnRhythmEngineSimulationPass();
	}
}