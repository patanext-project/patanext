using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Game.Scenar;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Scenar
{
	[DontInjectSystemToWorld]
	public abstract class ScenarScriptServer : GameAppSystem, IScenar
		//where TScenar : struct, IComponentData
	{
		protected GameEntity ScenarInGameWorld;

		protected ScenarScriptServer(WorldCollection wc) : base(wc)
		{
		}

		protected abstract Task OnStart();
		protected abstract Task OnLoop();
		protected abstract Task OnCleanup(bool reuse);

		protected GameEntity Self;
		protected GameEntity Creator;
		
		async Task IScenar.StartAsync(GameEntity self, GameEntity creator)
		{
			Self    = self;
			Creator = creator;
			
			await DependencyResolver.AsTask;
			ScenarInGameWorld = Safe(CreateEntity());
			
			await OnStart();
		}

		async Task IScenar.LoopAsync()
		{
			await DependencyResolver.AsTask;
			await OnLoop();
		}

		async Task IScenar.CleanupAsync(bool reuse)
		{
			await DependencyResolver.AsTask;
			await OnCleanup(reuse);

			if (!reuse)
			{
				Dispose();

				RemoveEntity(ScenarInGameWorld);
			}
		}
	}
}