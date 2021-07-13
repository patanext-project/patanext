using GameHost.Core;
using GameHost.Core.Ecs;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.BaseSystems
{
	[UpdateAfter(typeof(UpdateActiveAbilitySystem))]
	[UpdateAfter(typeof(ApplyAbilityStatisticOnChainingSystem))]
	public abstract class BaseAbilitySystem : GameAppSystem, IAbilitySimulationPass
	{
		public BaseAbilitySystem(WorldCollection collection) : base(collection)
		{
		}

		public abstract void OnAbilityUpdate();

		public void OnAbilitySimulationPass()
		{
			OnAbilityUpdate();
		}
	}
}