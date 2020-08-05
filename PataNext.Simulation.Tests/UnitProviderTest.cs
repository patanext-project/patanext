using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;

namespace PataNext.Module.Simulation.Tests
{
	public class UnitProviderTest : TestBootstrapBase
	{
		public void TestCreate()
		{
			var unitProvider = WorldCollection.GetOrCreate(wc => new PlayableUnitProvider(wc));

			var unit = unitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
			{
				Statistics = new UnitStatistics(),
				Direction  = UnitDirection.Right 
			});
		}
	}
}