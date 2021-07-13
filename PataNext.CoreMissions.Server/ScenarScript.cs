using GameHost.Core.Ecs;
using PataNext.Module.Simulation.Game.Scenar;

namespace PataNext.CoreMissions.Server
{
	public abstract class ScenarScript : ScenarScriptServer
	{
		protected ScenarScript(WorldCollection wc) : base(wc)
		{
		}
	}
}