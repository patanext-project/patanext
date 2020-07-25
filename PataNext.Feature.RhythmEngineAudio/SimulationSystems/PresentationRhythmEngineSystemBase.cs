using System;
using GameBase.Roles.Components;
using GameBase.Roles.Descriptions;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.RhythmEngine.Systems;

namespace PataNext.Simulation.Client.Systems
{
	[UpdateAfter(typeof(ProcessEngineSystem))]
	public class PresentationRhythmEngineSystemStart : AppSystem
	{
		public GameEntity LocalRhythmEngine { get; set; }

		private GameWorld gameWorld;
		public PresentationRhythmEngineSystemStart(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref gameWorld);
		}

		protected override void OnUpdate()
		{
			var playerEnumerator = gameWorld.QueryEntityWith(stackalloc[]
			{
				gameWorld.AsComponentType<PlayerDescription>(),
				gameWorld.AsComponentType<PlayerIsLocal>()
			});
			
			if (!playerEnumerator.TryGetFirst(out var localPlayerEntity))
				return;
			
			foreach (var entity in gameWorld.QueryEntityWith(stackalloc[]
			{
				gameWorld.AsComponentType<RhythmEngineDescription>(),
				gameWorld.AsComponentType<Relative<PlayerDescription>>()
			}))
			{
				if (gameWorld.GetComponentData<Relative<PlayerDescription>>(entity).Target != localPlayerEntity)
					continue;

				LocalRhythmEngine = entity;
				break;
			}
		}
	}
	
	[UpdateAfter(typeof(PresentationRhythmEngineSystemStart))]
	public class PresentationRhythmEngineSystemEnd : AppSystem
	{
		private PresentationRhythmEngineSystemStart start;
		
		public PresentationRhythmEngineSystemEnd(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref start);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			// make sure to invalidate it so that systems not depending on this group will not get invalid data that might crash the app later
			start.LocalRhythmEngine = default;
		}
	}
	
	[RestrictToApplication(typeof(SimulationApplication))]
	[UpdateAfter(typeof(PresentationRhythmEngineSystemStart)), UpdateBefore(typeof(PresentationRhythmEngineSystemEnd))]
	public abstract class PresentationRhythmEngineSystemBase : AppSystem
	{
		private PresentationRhythmEngineSystemStart start;

		public GameEntity LocalEngine => start.LocalRhythmEngine;
		
		protected PresentationRhythmEngineSystemBase(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref start);
		}
	}
}