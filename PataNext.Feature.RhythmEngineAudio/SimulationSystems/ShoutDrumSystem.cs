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

namespace PataNext.Simulation.Client.Systems
{
	[RestrictToApplication(typeof(SimulationApplication))]
	[UpdateAfter(typeof(ProcessEngineSystem))]
	public class ShoutDrumSystem : AppSystem
	{
		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureDrum =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureVoice =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureSlider =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private LoadAudioSystem   loadAudio;
		private CustomModule              module;
		private GameWorld gameWorld;

		public ShoutDrumSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref module);
			DependencyResolver.Add(() => ref gameWorld);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			var storage = new StorageCollection {module.DllStorage, module.Storage.Value}
			              .GetOrCreateDirectoryAsync("Sounds/RhythmEngine/Drums")
			              .Result;

			// TODO: should we have a configuration file for mapping the audio? (instead of hardcoding the mapping)
			for (var key = 1; key != 5; key++)
			{
				audioOnPressureDrum[key]   = new PooledDictionary<int, ResourceHandle<AudioResource>>();
				audioOnPressureVoice[key]  = new PooledDictionary<int, ResourceHandle<AudioResource>>();
				audioOnPressureSlider[key] = new PooledDictionary<int, ResourceHandle<AudioResource>>();

				for (var rank = 0; rank != 3; rank++)
				{
					audioOnPressureDrum[key][rank]  = loadAudio.Load($"drum_{key}_{rank}.ogg", storage);
					audioOnPressureVoice[key][rank] = loadAudio.Load($"voice_{key}_{rank}.wav", storage);
				}

				for (var rank = 0; rank != 2; rank++)
				{
					audioOnPressureSlider[key][rank] = loadAudio.Load($"drum_{key}_p{rank}.wav", storage);
				}
			}
		}

		private Entity audioPlayer;

		protected override void OnInit()
		{
			base.OnInit();

			audioPlayer = World.Mgr.CreateEntity();
			AudioPlayerUtility.Initialize(audioPlayer, new StandardAudioPlayerComponent());
		}

		protected override void OnUpdate()
		{
			if (!gameWorld.TryGetSingleton(out GameTime gameTime))
				return;

			var playerEnumerator = gameWorld.QueryEntityWith(stackalloc[]
			{
				gameWorld.AsComponentType<PlayerDescription>(),
				gameWorld.AsComponentType<PlayerIsLocal>()
			});
			
			if (!playerEnumerator.TryGetFirst(out var localPlayerEntity))
				return;

			var rhythmEngineEntity = default(GameEntity);
			foreach (var entity in gameWorld.QueryEntityWith(stackalloc[]
			{
				gameWorld.AsComponentType<RhythmEngineDescription>(),
				gameWorld.AsComponentType<Relative<PlayerDescription>>()
			}))
			{
				if (gameWorld.GetComponentData<Relative<PlayerDescription>>(entity).Target != localPlayerEntity)
					continue;

				rhythmEngineEntity = entity;
				break;
			}

			if (rhythmEngineEntity == default)
				return;

			foreach (var entity in gameWorld.QueryEntityWith(stackalloc[] {gameWorld.AsComponentType<PlayerInputComponent>()}))
			{
				var next = gameWorld.GetComponentData<PlayerInputComponent>(entity);
				
				var score = 0;
				var state = gameWorld.GetComponentData<RhythmEngineLocalState>(rhythmEngineEntity);
				var settings = gameWorld.GetComponentData<RhythmEngineSettings>(rhythmEngineEntity);
				if (Math.Abs(RhythmEngineUtility.GetScore(state, settings)) > FlowPressure.Perfect)
						score++;

				/*var cmdBuffer = gameWorld.GetBuffer<RhythmEngineLocalCommandBuffer>(rhythmEngineEntity);
				var isFirstInput = cmdBuffer.Span.Length == 0;*/

				for (var i = 0; i < next.Actions.Length; i++)
				{
					ref readonly var action = ref next.Actions[i];
					if (!action.InterFrame.AnyUpdate(gameTime.Frame))
						continue;
					
					if (action.InterFrame.HasBeenReleased(gameTime.Frame)/* && (!action.IsSliding || isFirstInput)*/)
						continue;

					ResourceHandle<AudioResource> resourceHandle;
					/*if (action.IsSliding)
						resourceHandle = audioOnPressureSlider[i + 1][score];
					else*/
						resourceHandle = audioOnPressureDrum[i + 1][score];

					AudioPlayerUtility.SetResource(audioPlayer, resourceHandle);
					AudioPlayerUtility.Play(audioPlayer);
				}
			}
		}
	}
}