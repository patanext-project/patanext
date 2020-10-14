using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine
{
	public struct RhythmSummonEnergy : IComponentData
	{
		public int Value;

		public static void AddToEntity(GameWorld gameWorld, GameEntity gameEntity, int maxEnergy = 1000)
		{
			gameWorld.AddComponent(gameEntity, new RhythmSummonEnergy());
			gameWorld.AddComponent(gameEntity, new RhythmSummonEnergyMax {MaxValue = maxEnergy});
		}
		
		public class Register : RegisterGameHostComponentData<RhythmSummonEnergy>
		{}
	}

	public struct RhythmSummonEnergyMax : IComponentData
	{
		public int MaxValue;
		
		public class Register : RegisterGameHostComponentData<RhythmSummonEnergyMax>
		{}
	}
}