using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Game.Scenar;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Scenar
{
	public abstract class ScenarScriptServer<TScenar> : GameAppSystem, IScenar
		where TScenar : struct, IComponentData
	{
		protected ScenarScriptServer(WorldCollection collection) : base(collection)
		{
		}

		public abstract Task Start();
		public abstract Task Loop();
		public abstract Task Cleanup();
	}
}