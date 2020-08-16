using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[UpdateAfter(typeof(ManageComponentTagSystem))]
	public class ProcessEngineSystem : RhythmEngineSystemBase
	{
		private IManagedWorldTime worldTime;

		public ProcessEngineSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		public override void OnRhythmEngineSimulationPass()
		{
			if (!GameWorld.TryGetSingleton(out GameTime gameTime))
				return;

			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<RhythmEngineController>(),
				GameWorld.AsComponentType<RhythmEngineLocalState>(),
				GameWorld.AsComponentType<RhythmEngineSettings>()
			}))
			{
				ref readonly var controller = ref GameWorld.GetComponentData<RhythmEngineController>(entity);
				ref readonly var settings   = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);
				ref var          state      = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);

				// store the previous elapsed time, it will be used for checking new beats
				var previous      = state.Elapsed;
				var previousBeats = RhythmEngineUtility.GetActivationBeat(previous, settings.BeatInterval);

				state.Elapsed = worldTime.Total - controller.StartTime;
				if (state.Elapsed < TimeSpan.Zero && HasComponent<RhythmEngineIsPlaying>(entity))
					GameWorld.RemoveComponent(entity, AsComponentType<RhythmEngineIsPlaying>());

				var currentBeats = RhythmEngineUtility.GetActivationBeat(state, settings);
				if (previousBeats != currentBeats)
					state.NewBeatTick = (uint) gameTime.Frame;
				state.CurrentBeat = currentBeats;

				if (!HasComponent<RhythmEngineIsPlaying>(entity))
				{
					state.RecoveryActivationBeat = -1;
					state.LastPressure           = default;
				}
			}

			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[]
			{
				AsComponentType<GameCombo.Settings>(),
				AsComponentType<GameCombo.State>(),
				AsComponentType<RhythmSummonEnergy>()
			}))
			{
				if (!GetComponentData<GameCombo.Settings>(entity).CanEnterFever(GetComponentData<GameCombo.State>(entity)))
					GetComponentData<RhythmSummonEnergy>(entity) = default;
			}
		}
	}
}