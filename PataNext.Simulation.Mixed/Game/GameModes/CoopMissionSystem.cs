using System;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.IO;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Utility;
using PataNext.Game.Scenar;

namespace PataNext.Module.Simulation.GameModes
{
	public class CoopMissionSystem : MissionGameModeBase<CoopMission>
	{
		public CoopMissionSystem(WorldCollection collection) : base(collection)
		{
		}

		private ResourceHandle<ScenarResource> scenarEntity;

		/*protected override async Task GameModeStartRound()
		{
			// Load Scenar
			var scenarRequest = World.Mgr.CreateEntity();
			scenarRequest.Set(new ScenarLoadRequest("MyScenar"));

			while (!scenarEntity.IsLoaded)
				await Task.Yield();

			await scenarEntity.Result.Interface.Start();
		}

		protected override async Task GameModePlayLoop()
		{
			// Either the resource got destroyed or something worst happened
			if (!scenarEntity.IsLoaded)
			{
				RequestEndRound();
				return;
			}

			await scenar.Loop();
			
			RequestEndRound();
		}*/
	}

	public struct CoopMission : IComponentData
	{
	}
}