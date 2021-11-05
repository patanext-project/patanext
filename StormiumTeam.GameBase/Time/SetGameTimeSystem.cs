using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Time;
using GameHost.Worlds.Components;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Time
{
	/// <summary>
	/// Update <see cref="GameTime"/> from an <see cref="IManagedWorldTime"/>
	/// </summary>
	public class SetGameTimeSystem : GameAppSystem, IPreUpdateSimulationPass
	{
		private IManagedWorldTime managedWorldTime;

		public SetGameTimeSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref managedWorldTime);
		}

		private GameEntityHandle timeEntity;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			timeEntity = GameWorld.CreateEntity();
			GameWorld.AddComponent<GameTime>(timeEntity);
		}

		public void OnBeforeSimulationUpdate()
		{
			ref var gameTime = ref GameWorld.GetComponentData<GameTime>(timeEntity);
			gameTime.Frame++;
			gameTime.Delta   = (float) managedWorldTime.Delta.TotalSeconds;
			gameTime.Elapsed = managedWorldTime.Total.TotalSeconds;
		}
	}
}