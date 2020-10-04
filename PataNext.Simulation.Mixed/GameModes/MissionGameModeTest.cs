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

			GameWorld.AddComponent(GameWorld.CreateEntity(), new MissionGameModeTest());
		}

		protected override async Task GameModePlayLoop()
		{
			await Task.Delay(100);
			Console.WriteLine(Thread.CurrentThread.Name);
		}
	}
}