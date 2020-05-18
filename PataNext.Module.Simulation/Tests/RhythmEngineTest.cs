using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Entities;
using GameHost.Input.OpenTKBackend;
using OpenToolkit.Windowing.Common.Input;
using PataNext.Module.Simulation.RhythmEngine;
using PataNext.Module.Simulation.RhythmEngine.Data;

namespace PataNext.Module.Simulation.Tests
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	public class RhythmEngineTest : AppSystem
	{
		struct OnInput
		{
			public int Key;
		}

		[RestrictToApplication(typeof(GameInputThreadingHost))]
		public class RestrictedSystem : AppSystem
		{
			private OpenTkInputBackend inputBackend;
			private WorldCollection    targetAppWorld;
			private IScheduler         targetAppScheduler;

			private Dictionary<Key, Action> actionPerKey;

			public RestrictedSystem(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref inputBackend);

				actionPerKey = new Dictionary<Key, Action>()
				{
					{Key.Keypad4, () => targetAppWorld.Mgr.CreateEntity().Set(new OnInput {Key = 1})},
					{Key.Keypad6, () => targetAppWorld.Mgr.CreateEntity().Set(new OnInput {Key = 2})},
					{Key.Keypad2, () => targetAppWorld.Mgr.CreateEntity().Set(new OnInput {Key = 3})},
					{Key.Keypad8, () => targetAppWorld.Mgr.CreateEntity().Set(new OnInput {Key = 4})},
				};
			}

			protected override void OnUpdate()
			{
				if (ThreadingHost.TypeToThread.TryGetValue(typeof(GameSimulationThreadingHost), out var host))
				{
					var app = (GameSimulationThreadingHost) host.Host;

					foreach (var world in app.MappedWorldCollection)
					{
						targetAppWorld     = world.Value;
						targetAppScheduler = app.GetScheduler();
						break;
					}
				}

				if (targetAppWorld == null)
					return;

				foreach (var kvp in actionPerKey)
					if (inputBackend.IsKeyDown(kvp.Key))
						targetAppScheduler.Add(kvp.Value);
			}
		}

		private EntitySet inputSet, engineSet;

		public RhythmEngineTest(WorldCollection collection) : base(collection)
		{
			inputSet = collection.Mgr.GetEntities()
			                     .With<OnInput>()
			                     .AsSet();
			engineSet = collection.Mgr.GetEntities()
			                      .With<RhythmEngineController>()
			                      .With<RhythmEngineLocalState>()
			                      .With<RhythmEngineLocalCommandBuffer>()
			                      .AsSet();
		}

		protected override void OnUpdate()
		{
			foreach (ref readonly var entity in engineSet.GetEntities())
			{
				ref var state = ref entity.Get<RhythmEngineLocalState>();
				var settings = entity.Get<RhythmEngineSettings>();
				var buffer = entity.Get<RhythmEngineLocalCommandBuffer>();
				foreach (ref var input in World.Mgr.Get<OnInput>())
				{
					World.Mgr.CreateEntity();
					
					
					var pressure = new FlowPressure(input.Key, state.Elapsed, settings.BeatInterval);
					buffer.Add(pressure);
					state.LastPressure = pressure;

					//Console.WriteLine($"(key={pressure.KeyId}, flowbeat={pressure.FlowBeat}, score={pressure.Score})");
				}
			}

			inputSet.DisposeAllEntities();
		}
	}
}