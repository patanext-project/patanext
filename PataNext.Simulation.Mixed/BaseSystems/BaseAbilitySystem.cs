using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.GameBase.SystemBase;

namespace PataNext.Module.Simulation.BaseSystems
{
	public class BaseAbilitySystem : GameAppSystem
	{
		public BaseAbilitySystem(WorldCollection collection) : base(collection)
		{
		}
	}
}