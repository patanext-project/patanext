using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultEcs;
using DefaultEcs.Serialization;
using GameHost;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Input;
using GameHost.Input.Default;
using Microsoft.Extensions.Logging;
using PataNext.Module.RhythmEngine;
using PataNext.Module.RhythmEngine.Data;
using PataponGameHost.Inputs;

namespace PataNext.Module.Simulation.Tests
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	public class OnInputForRhythmEngine : AppSystem
	{
		public OnInputForRhythmEngine(WorldCollection collection) : base(collection)
		{
		}

		private EntitySet playerSet;
		private EntitySet rhythmEngineSet;

		protected override void OnInit()
		{
			base.OnInit();

			playerSet = World.Mgr.GetEntities()
			                 .With<PlayerInput>()
			                 .AsSet();
			rhythmEngineSet = World.Mgr.GetEntities()
			                       .With<RhythmEngineController>()
			                       .AsSet();
		}

		public override bool CanUpdate() => base.CanUpdate() && playerSet.Count > 0 && rhythmEngineSet.Count > 0;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			// solo only for now
			ref readonly var playerInput = ref playerSet.GetEntities()[0].Get<PlayerInput>();

			foreach (ref readonly var entity in rhythmEngineSet.GetEntities())
			{
				ref var          state    = ref entity.Get<RhythmEngineLocalState>();
				ref readonly var settings = ref entity.Get<RhythmEngineSettings>();

				var buffer = entity.Get<RhythmEngineLocalCommandBuffer>();
				for (var i = 0; i < playerInput.Actions.Length; i++)
				{
					ref readonly var action = ref playerInput.Actions[i];
					if (!action.FrameUpdate)
						continue;

					// If this is not the end of a slider or if it is but our command buffer is empty, skip it.
					if (action.WasReleased && (!action.IsSliding || buffer.Count == 0))
						continue;

					var pressure = new FlowPressure(i + 1, state.Elapsed, settings.BeatInterval)
					{
						IsSliderEnd = action.IsSliding
					};

					buffer.Add(pressure);
					state.LastPressure = pressure;
				}
			}
		}
	}
}