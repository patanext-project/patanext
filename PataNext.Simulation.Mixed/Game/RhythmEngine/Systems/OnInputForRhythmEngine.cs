using System;
using GameBase.Time.Components;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	public class OnInputForRhythmEngine : RhythmEngineSystemBase
	{
		public OnInputForRhythmEngine(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnUpdate()
		{
			if (!GameWorld.TryGetSingleton(out GameTime gameTime))
				return;
			
			// solo only for now
			if (!GameWorld.TryGetSingleton(out PlayerInputComponent playerInput))
				return;

			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<RhythmEngineIsPlaying>(),
				GameWorld.AsComponentType<RhythmEngineController>(),
				GameWorld.AsComponentType<RhythmEngineLocalState>(),
				GameWorld.AsComponentType<RhythmEngineSettings>(),
				GameWorld.AsComponentType<RhythmEngineLocalCommandBuffer>()
			}))
			{
				ref var state = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);
				ref readonly var settings = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);

				var buffer = GameWorld.GetBuffer<RhythmEngineLocalCommandBuffer>(entity);
				for (var i = 0; i < playerInput.Actions.Length; i++)
				{
					ref readonly var action = ref playerInput.Actions[i];
					if (!action.InterFrame.AnyUpdate(gameTime.Frame))
						continue;

					// If this is not the end of a slider or if it is but our command buffer is empty, skip it.
					Console.WriteLine(action.IsSliding);
					if (action.InterFrame.HasBeenReleased(gameTime.Frame) && (!action.IsSliding || buffer.Span.Length == 0))
						continue;

					var pressure = new FlowPressure(i + 1, state.Elapsed, settings.BeatInterval)
					{
						IsSliderEnd = action.IsSliding
					};

					buffer.Add(new RhythmEngineLocalCommandBuffer {Value = pressure});
					state.LastPressure = pressure;
				}
			}
		}
	}
}