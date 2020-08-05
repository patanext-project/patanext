using GameHost.Core.Ecs;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.BaseSystems
{
	public abstract class BaseAbilitySystem : GameAppSystem, IAbilityPreSimulationPass
	{
		public BaseAbilitySystem(WorldCollection collection) : base(collection)
		{
		}

		public abstract void OnAbilityPreSimulationPass();
	}
}