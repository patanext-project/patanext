using System;
using System.Numerics;
using System.Xml;
using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Worlds.Components;

namespace PataNext.Module.Simulation
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class CreateEntitySystem : AppSystem
	{
		private GameWorld gameWorld;
		private IManagedWorldTime worldTime;
		
		public CreateEntitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref gameWorld);
			DependencyResolver.Add(() => ref worldTime);
		}

		protected override void OnInit()
		{
			base.OnInit();

			for (var i = 0; i != 3; i++)
			{
				var ent = gameWorld.CreateEntity();
				gameWorld.AddComponent(ent, new Position());
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			foreach (ref readonly var entity in gameWorld.Boards.Entity.Alive)
			{
				if (!gameWorld.HasComponent<Position>(entity))
					continue;

				ref var position = ref gameWorld.GetComponentData<Position>(entity);
				position.Value.X += (float) worldTime.Delta.TotalSeconds;
			}
		}
	}

	public struct Position : IComponentData
	{
		public Vector3 Value;

		public Position(Vector3 value)
		{
			Value = value;
		}
	}
}