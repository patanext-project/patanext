using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameBase.Roles.Components;
using GameBase.Roles.Descriptions;
using GameBase.Time.Components;
using GameHost.Audio.Features;
using GameHost.Audio.Players;
using GameHost.Audio.Systems;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.IO;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Feature.RhythmEngineAudio;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.RhythmEngine;
using PataNext.Module.Simulation.Game.RhythmEngine.Systems;
using PataNext.Simulation.Client.Systems.Inputs;

namespace PataNext.Simulation.Client.Systems
{
	public class ShoutDrumSystem : PresentationRhythmEngineSystemBase
	{
		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureDrum =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureVoice =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureSlider =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private LoadAudioResourceSystem loadAudioResource;
		private CustomModule    module;
		private GameWorld       gameWorld;

		public ShoutDrumSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref loadAudioResource);
			DependencyResolver.Add(() => ref module);
			DependencyResolver.Add(() => ref gameWorld);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			var drumStorage = new StorageCollection {module.DllStorage, module.Storage.Value}
			              .GetOrCreateDirectoryAsync("Sounds/RhythmEngine/Drums")
			              .Result;
			var effectStorage = new StorageCollection {module.DllStorage, module.Storage.Value}
			                  .GetOrCreateDirectoryAsync("Sounds/RhythmEngine/Effects")
			                  .Result;
			
			AudioPlayerUtility.Initialize(commandAudioPlayer   = World.Mgr.CreateEntity(), new StandardAudioPlayerComponent());
			AudioPlayerUtility.Initialize(onPerfectAudioPlayer = World.Mgr.CreateEntity(), new StandardAudioPlayerComponent());

			// TODO: should we have a configuration file for mapping the audio? (instead of hardcoding the mapping)
			for (var key = 1; key != 5; key++)
			{
				audioOnPressureDrum[key]   = new PooledDictionary<int, ResourceHandle<AudioResource>>();
				audioOnPressureVoice[key]  = new PooledDictionary<int, ResourceHandle<AudioResource>>();
				audioOnPressureSlider[key] = new PooledDictionary<int, ResourceHandle<AudioResource>>();

				for (var rank = 0; rank != 3; rank++)
				{
					audioOnPressureDrum[key][rank]  = loadAudioResource.Load($"drum_{key}_{rank}.ogg", drumStorage);
					audioOnPressureVoice[key][rank] = loadAudioResource.Load($"voice_{key}_{rank}.wav", drumStorage);
				}

				for (var rank = 0; rank != 2; rank++)
				{
					audioOnPressureSlider[key][rank] = loadAudioResource.Load($"drum_{key}_p{rank}.wav", drumStorage);
				}
			}
			
			AudioPlayerUtility.SetResource(onPerfectAudioPlayer, loadAudioResource.Load("on_perfect.wav", effectStorage));
		}

		private Entity commandAudioPlayer;
		private Entity onPerfectAudioPlayer;

		public override bool CanUpdate()
		{
			return LocalEngine != default && base.CanUpdate();
		}

		protected override void OnUpdate()
		{
			if (!gameWorld.TryGetSingleton(out GameTime gameTime))
				return;

			foreach (var entity in gameWorld.QueryEntityWith(stackalloc[] {gameWorld.AsComponentType<PlayerInputComponent>()}))
			{
				var next = gameWorld.GetComponentData<PlayerInputComponent>(entity);

				var score    = 0;
				var state    = gameWorld.GetComponentData<RhythmEngineLocalState>(LocalEngine);
				var settings = gameWorld.GetComponentData<RhythmEngineSettings>(LocalEngine);
				if (Math.Abs(RhythmEngineUtility.GetScore(state, settings)) > FlowPressure.Perfect)
					score++;
				
				if (state.IsRecovery(RhythmEngineUtility.GetFlowBeat(state, settings)))
					score = 2;

				var cmdBuffer    = gameWorld.GetBuffer<RhythmEngineLocalCommandBuffer>(LocalEngine);
				var executing    = gameWorld.GetComponentData<RhythmEngineExecutingCommand>(LocalEngine);
				
				// since this system may update after CmdBuffer get clear, we need to check if we have an active command target to determine if we can use a slider sound
				var isFirstInput = cmdBuffer.Span.Length == 0 
				                   && (executing.CommandTarget == default || executing.ActivationBeatStart <= RhythmEngineUtility.GetActivationBeat(state, settings));

				for (var i = 0; i < next.Actions.Length; i++)
				{
					ref readonly var action = ref next.Actions[i];
					if (!action.InterFrame.AnyUpdate(gameTime.Frame))
						continue;
					
					if (action.InterFrame.HasBeenReleased(gameTime.Frame) && (!action.IsSliding || isFirstInput))
						continue;

					ResourceHandle<AudioResource> resourceHandle;
					if (action.IsSliding)
						resourceHandle = audioOnPressureSlider[i + 1][score];
					else
						resourceHandle = audioOnPressureDrum[i + 1][score];
					
					AudioPlayerUtility.SetResource(commandAudioPlayer, resourceHandle);
					AudioPlayerUtility.Play(commandAudioPlayer);
					
					if (executing.PowerInteger >= 100
					    && executing.ActivationBeatStart >= RhythmEngineUtility.GetActivationBeat(state, settings))
						AudioPlayerUtility.Play(onPerfectAudioPlayer);
				}
			}
		}
	}
}