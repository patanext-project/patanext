using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Utility;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameModes
{
	public class StartYaridaTrainingGameMode : GameAppSystem
	{
		private TaskScheduler taskScheduler;

		private YaridaTrainingGameMode trainingGameMode;

		private const int targetFrame = 10;

		public StartYaridaTrainingGameMode(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref trainingGameMode);
			DependencyResolver.Add(() => ref taskScheduler);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			/*TaskRunUtility.StartUnwrap(async (ccs) =>
			{
				for (var frame = 0; frame < targetFrame; frame++)
					await Task.Yield();
				
				var playerEntity = CreateEntity();
				AddComponent(playerEntity, new PlayerDescription());
				AddComponent(playerEntity, new PlayerIsLocal());

				AddComponent(CreateEntity(), new AtCityGameModeData());
			}, taskScheduler, CancellationToken.None);*/
			TaskRunUtility.StartUnwrap(async (ccs) =>
			{
				for (var frame = 0; frame < targetFrame; frame++)
					await Task.Yield();

				trainingGameMode.Start(64);
			}, taskScheduler, CancellationToken.None).ContinueWith(t =>
			{
				Console.WriteLine($"{t.Exception}");
			});
		}
	}
}