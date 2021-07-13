using System;
using System.Collections.Generic;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Network.Components;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Module.Simulation.Game.RhythmEngine.Systems
{
	[UpdateAfter(typeof(ManageComponentTagSystem))]
	public class ProcessEngineSystem : RhythmEngineSystemBase
	{
		private IManagedWorldTime    worldTime;
		private InstigatorTimeSystem instigatorTimeSystem;
		
		public ProcessEngineSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref instigatorTimeSystem);
		}

		private EntityQuery engineQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			engineQuery = CreateEntityQuery(new[]
			{
				typeof(RhythmEngineController),
				typeof(RhythmEngineLocalState),
				typeof(RhythmEngineSettings)
			});
		}

		private WorldTime previousWorldTime;
		public override void OnRhythmEngineSimulationPass()
		{
			if (!GameWorld.TryGetSingleton(out GameTime gameTime))
				return;


			var delta = worldTime.Total - previousWorldTime.Total;

			foreach (ref var entity in engineQuery)
			{
				ref readonly var controller = ref GameWorld.GetComponentData<RhythmEngineController>(entity);
				ref readonly var settings   = ref GameWorld.GetComponentData<RhythmEngineSettings>(entity);
				ref var          state      = ref GameWorld.GetComponentData<RhythmEngineLocalState>(entity);

				// store the previous elapsed time, it will be used for checking new beats
				var previous      = state.Elapsed;
				var previousBeats = RhythmEngineUtility.GetActivationBeat(previous, settings.BeatInterval);

				TryGetComponentData(entity, out AssignInstigatorTime assignInstigatorTime);
				var (_, time) = instigatorTimeSystem.GetTime(assignInstigatorTime.Instigator);

				if (controller.StartTime != state.PreviousStartTime)
				{
					state.PreviousStartTime = controller.StartTime;
					// TODO: diff should be dynamically applicated
					state.Elapsed = time.currentInterpolated - controller.StartTime + time.diffInterpolated;
				}

				if (time.diffInterpolated < TimeSpan.Zero)
					time.diffInterpolated = TimeSpan.Zero;
				else
					time.diffInterpolated = TimeSpan.FromTicks(Math.Min(time.diffInterpolated.Ticks, TimeSpan.FromSeconds(0.00025f).Ticks));

				// difference will be applied very minimaly
				var original = state.Elapsed;
				var prev     = state.Elapsed;
				
				state.Elapsed += delta + (time.diffInterpolated * 0.001f);
				// increasing the time made us a bit ahead, so take the previous one and decrease the delta factor
				if (state.Elapsed > time.currentInterpolated - controller.StartTime)
				{
					var factor = 0.999f;
					if (state.Elapsed - time.currentInterpolated - controller.StartTime > TimeSpan.FromSeconds(1))
					{
						factor = 0.99f;
					}

					if (state.Elapsed - time.currentInterpolated - controller.StartTime > TimeSpan.FromSeconds(2))
					{
						factor = 0.98f;
					}

					prev          += delta * factor;
					state.Elapsed =  prev;
				}
				else if (state.Elapsed < time.currentInterpolated - controller.StartTime)
				{
					var factor = 1.0005f;
					if (state.Elapsed - time.currentInterpolated - controller.StartTime > TimeSpan.FromSeconds(1))
					{
						factor = 1.001f;
					}

					if (state.Elapsed - time.currentInterpolated - controller.StartTime > TimeSpan.FromSeconds(2))
					{
						factor = 1.0015f;
					}

					prev          += delta * factor;
					state.Elapsed =  prev;
				}

				if (state.Elapsed < TimeSpan.Zero && HasComponent<RhythmEngineIsPlaying>(entity))
				{
					GameWorld.RemoveComponent(entity, AsComponentType<RhythmEngineIsPlaying>());
					
					state.RecoveryActivationBeat = -1;
					state.LastPressure           = default;

					// swap back
					entity = default;
					continue;
				}

				var currentBeats = RhythmEngineUtility.GetActivationBeat(state, settings);
				if (previousBeats != currentBeats)
					state.NewBeatTick = (uint) gameTime.Frame;
				state.CurrentBeat = currentBeats;
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

			previousWorldTime = worldTime.ToStruct();
		}
	}
}