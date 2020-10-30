using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Utility;
using PataNext.Module.Simulation.Components.GameModes;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameModes
{
	public class StartYaridaTrainingGameMode : GameAppSystem
	{
		private TaskScheduler taskScheduler;

		public StartYaridaTrainingGameMode(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref taskScheduler);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			AddComponent(CreateEntity(), new BasicTestGameMode());

			/*TaskRunUtility.StartUnwrap(async (ccs) =>
			{
				for (var frame = 0; frame < targetFrame; frame++)
					await Task.Yield();
				
				var playerEntity = CreateEntity();
				AddComponent(playerEntity, new PlayerDescription());
				AddComponent(playerEntity, new PlayerIsLocal());

				AddComponent(CreateEntity(), new AtCityGameModeData());
			}, taskScheduler, CancellationToken.None);*/
		}
	}
}