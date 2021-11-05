using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GameModes;

namespace PataNext.Module.Simulation.GameModes.Versus
{
	public class VsPayloadGameModeSystem : MissionGameModeBase<VsPayloadGameMode>
	{
		private PayloadMoveSystem payloadMoveSystem;

		public VsPayloadGameModeSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref payloadMoveSystem);
		}

		private EntityQuery toDestroyQuery;

		protected override async Task GameModeStartRound()
		{
			// Make sure to destroy what we must destroy
			toDestroyQuery.RemoveAllEntities();
			
			// Create the payload
		}

		protected override Task GameModePlayLoop()
		{
			payloadMoveSystem.Update();

			return Task.CompletedTask;
		}
	}
}