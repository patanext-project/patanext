using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Arena
{
	public abstract class ArenaBase : GameAppSystem
	{
		protected ArenaBase(WorldCollection collection) : base(collection)
		{
		}
	}
}