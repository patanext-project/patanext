using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using NUnit.Framework;

namespace PataNext.Module.Simulation.Tests
{
	public abstract class TestBootstrapBase
	{
		public WorldCollection WorldCollection;
		public Scheduler       Scheduler;
		public GameWorld       GameWorld;

		public ManagedWorldTime WorldTime;

		[SetUp]
		public void SetUp()
		{
			WorldCollection = new WorldCollection(null, new World());

			var context = WorldCollection.Ctx;
			context.BindExisting<IScheduler>(Scheduler        = new Scheduler());
			context.BindExisting<IManagedWorldTime>(WorldTime = new ManagedWorldTime());
			context.BindExisting(GameWorld                    = new GameWorld());
		}

		public void RunScheduler()
		{
			for (var i = 0; i != 4; i++)
				Scheduler.Run();
		}
	}
}