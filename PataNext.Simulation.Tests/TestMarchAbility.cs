using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using NUnit.Framework;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Simulation.Mixed.Abilities.Defaults;
using PataNext.Simulation.Mixed.Abilities.Subset;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.Module.Simulation.Tests
{
	public class TestMarchAbility
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

		private void RunScheduler()
		{
			for (var i = 0; i != 4; i++)
				Scheduler.Run();
		}

		[Test]
		public void TestIsActive()
		{
			WorldCollection.GetOrCreate(wc => new DefaultMarchAbilitySystem(wc));
			WorldCollection.GetOrCreate(wc => new DefaultSubsetMarchAbilitySystem(wc));
			
			var marchAbilityProvider = WorldCollection.GetOrCreate(wc => new DefaultMarchAbilityProvider(wc));
			
			RunScheduler();

			var target = GameWorld.CreateEntity();
			GameWorld.AddComponent(target, new Position());
			
			var unit = GameWorld.CreateEntity();
			GameWorld.AddComponent(unit, new Position());
			GameWorld.AddComponent(unit, new Velocity());
			GameWorld.AddComponent(unit, new UnitPlayState
			{
				Weight = 10,
				MovementSpeed = 10
			});
			GameWorld.AddComponent(unit, new UnitControllerState());
			GameWorld.AddComponent(unit, UnitDirection.Right);
			GameWorld.AddComponent(unit, new UnitTargetOffset());
			GameWorld.AddComponent(unit, new UnitTargetControlTag());
			GameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(target));
			
			var ability = marchAbilityProvider.SpawnEntityWithArguments(new CreateAbility
			{
				Owner     = unit,
				Selection = AbilitySelection.Horizontal
			});
			GameWorld.GetComponentData<AbilityState>(ability).Phase = EAbilityPhase.Active;
			
			RunScheduler();
			
			WorldTime.Delta = TimeSpan.FromSeconds(1f / 60f);
			for (var i = 0; i != 1; i++)
				WorldCollection.LoopPasses();

			Assert.Greater(GameWorld.GetComponentData<Velocity>(unit).Value.X, 0.1);
			Assert.Greater(GameWorld.GetComponentData<Position>(target).Value.X, 0.1);
		}
	}
}