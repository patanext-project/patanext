using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Game.Scenar;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics;

namespace PataNext.CoreMissions.Server
{
	public abstract class ScenarScript : ScenarScriptServer
	{
		protected IPhysicsSystem PhysicsSystem;
		
		protected ResPathGen ResPathGen;

		protected GameResourceDb<GameGraphicResource> GraphicDb;

		protected ScenarScript(WorldCollection wc) : base(wc)
		{
			DependencyResolver.Add(() => ref PhysicsSystem);
			
			DependencyResolver.Add(() => ref ResPathGen);
			
			DependencyResolver.Add(() => ref GraphicDb);
		}
	}
}