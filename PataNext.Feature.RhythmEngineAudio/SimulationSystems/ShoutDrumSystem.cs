using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Audio.Features;
using GameHost.Audio.Players;
using GameHost.Audio.Systems;
using GameHost.Core.Ecs;
using GameHost.IO;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using PataNext.Feature.RhythmEngineAudio;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Game.RhythmEngine;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Simulation.Client.Systems
{
	public class ShoutDrumSystem : PresentationRhythmEngineSystemBase
	{
		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureDrum =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureSlider =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureVoice =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private EntityQuery abilityQuery;

		private ResourceHandle<AudioResource> audioOnHeroMode;

		private Entity    commandAudioPlayer;
		private GameWorld gameWorld;

		private LoadAudioResourceSystem loadAudioResource;
		private CustomModule            module;
		private Entity                  onPerfectAudioPlayer;

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

				for (var rank = 0; rank != 2; rank++) audioOnPressureSlider[key][rank] = loadAudioResource.Load($"drum_{key}_p{rank}.wav", drumStorage);
			}

			audioOnHeroMode = loadAudioResource.Load("voice_on_hero_mode.wav", drumStorage);

			AudioPlayerUtility.SetResource(onPerfectAudioPlayer, loadAudioResource.Load("on_perfect.wav", effectStorage));
		}

		public override bool CanUpdate()
		{
			return GameWorld.Contains(LocalEngine) && LocalInformation.Elapsed >= TimeSpan.Zero && base.CanUpdate();
		}

		protected override void OnUpdatePass()
		{
			if (!CanUpdate())
				return;

			if (!gameWorld.TryGetSingleton(out GameTime gameTime))
				return;

			// Check for any abilities of our that are triggering hero mode.
			var isHeroModeIncoming = false;
			{
				var stateAccessor      = new ComponentDataAccessor<AbilityState>(GameWorld);
				var activationAccessor = new ComponentDataAccessor<AbilityActivation>(GameWorld);
				foreach (var ability in abilityQuery ??= CreateEntityQuery(new[]
				{
					AsComponentType<AbilityState>(),
					AsComponentType<AbilityActivation>(),
					AsComponentType<Owner>()
				}))
					if (activationAccessor[ability].Type.HasFlag(EAbilityActivationType.HeroMode)
					    && stateAccessor[ability].Phase == EAbilityPhase.WillBeActive)
						isHeroModeIncoming = true;
			}

			foreach (var entity in gameWorld.QueryEntityWith(stackalloc[] {gameWorld.AsComponentType<GameRhythmInputComponent>()}))
			{
				var next = gameWorld.GetComponentData<GameRhythmInputComponent>(entity);

				var score    = 0;
				var state    = gameWorld.GetComponentData<RhythmEngineLocalState>(LocalEngine);
				var settings = gameWorld.GetComponentData<RhythmEngineSettings>(LocalEngine);
				if (Math.Abs(RhythmEngineUtility.GetScore(state, settings)) > FlowPressure.Perfect)
					score++;

				if (state.IsRecovery(RhythmEngineUtility.GetFlowBeat(state, settings)))
					score = 2;

				var cmdBuffer = gameWorld.GetBuffer<RhythmEngineCommandProgressBuffer>(LocalEngine);
				var executing = gameWorld.GetComponentData<RhythmEngineExecutingCommand>(LocalEngine);

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
					if (isHeroModeIncoming)
						resourceHandle = audioOnHeroMode;
					else if (action.IsSliding)
						resourceHandle = audioOnPressureSlider[i + 1][score];
					else
						resourceHandle = audioOnPressureDrum[i + 1][score];

					Console.WriteLine(isHeroModeIncoming);
					AudioPlayerUtility.SetResource(commandAudioPlayer, resourceHandle);
					AudioPlayerUtility.Play(commandAudioPlayer);

					if (executing.PowerInteger >= 100
					    && !isHeroModeIncoming
					    && executing.ActivationBeatStart >= RhythmEngineUtility.GetActivationBeat(state, settings))
						AudioPlayerUtility.Play(onPerfectAudioPlayer);
				}
			}
		}
	}
}