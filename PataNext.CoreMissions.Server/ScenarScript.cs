using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Game.Scenar;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Network.Authorities;
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

		public GameResource<GameGraphicResource> GetGraphic(string[] resource)
		{
			return GraphicDb.GetOrCreate(ResPathGen.Create(resource, ResPath.EType.ClientResource));
		}
	}
}