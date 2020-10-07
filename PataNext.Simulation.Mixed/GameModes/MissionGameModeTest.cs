using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.GameModes
{
	public struct MissionGameModeTest : IComponentData
	{
	}

	public class MissionGameModeTestSystem : MissionGameModeBase<MissionGameModeTest>
	{
		public MissionGameModeTestSystem(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			//GameWorld.AddComponent(GameWorld.CreateEntity(), new MissionGameModeTest());
		}

		protected override async Task GameModeInitialisation()
		{
			Console.WriteLine("----- Initialize gamemode");
		}

		protected override async Task GameModeStartRound()
		{
			Console.WriteLine("Start Round");
			await Task.Delay(100);
		}

		protected override async Task GameModePlayLoop()
		{
			Console.WriteLine(Thread.CurrentThread.Name);

			RequestEndRound();
		}

		protected override async Task GameModeEndRound()
		{
			Console.WriteLine("End Round");
			await Task.Delay(1000);
		}
	}
}