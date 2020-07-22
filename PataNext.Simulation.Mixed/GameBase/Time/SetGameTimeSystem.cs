using System.Collections.Generic;
using DefaultEcs;
using GameBase.Time.Components;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameBase.Time
{
	/// <summary>
	/// Update <see cref="GameTime"/> from an <see cref="IManagedWorldTime"/>
	/// </summary>
	public class SetGameTimeSystem : GameSystem
	{
		private IManagedWorldTime managedWorldTime;

		public SetGameTimeSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref managedWorldTime);
		}

		private GameEntity timeEntity;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			timeEntity = GameWorld.CreateEntity();
			GameWorld.AddComponent<GameTime>(timeEntity);
		}

		protected override void OnUpdate()
		{
			ref var gameTime = ref GameWorld.GetComponentData<GameTime>(timeEntity);
			gameTime.Frame++;
			gameTime.Delta   = (float) managedWorldTime.Delta.TotalSeconds;
			gameTime.Elapsed = managedWorldTime.Total.TotalSeconds;
		}
	}
}