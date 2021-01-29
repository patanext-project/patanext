using System.Collections.Generic;
using DefaultEcs;
using GameHost.Audio.Features;
using GameHost.Audio.Players;
using GameHost.Audio.Systems;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.IO;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Simulation.Client.Systems
{
	public class OnNewBeatSystem : PresentationRhythmEngineSystemBase
	{
		private Entity                  audioPlayer;
		private GameWorld               gameWorld;
		private LoadAudioResourceSystem loadAudio;
		private GameHostModule          module;

		private ResourceHandle<AudioResource> newBeatSound;

		public OnNewBeatSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref gameWorld);
			DependencyResolver.Add(() => ref module);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			newBeatSound = loadAudio.Load("Sounds/RhythmEngine/Effects/on_new_beat.ogg", new StorageCollection {module.DllStorage, module.Storage.Value});

			audioPlayer = World.Mgr.CreateEntity();
			AudioPlayerUtility.Initialize(audioPlayer, new StandardAudioPlayerComponent());
			AudioPlayerUtility.SetResource(audioPlayer, newBeatSound);
		}

		public override bool CanUpdate()
		{
			return LocalEngine != default && HasComponent<RhythmEngineIsPlaying>(LocalEngine) && base.CanUpdate();
		}

		protected override void OnUpdatePass()
		{
			if (!gameWorld.TryGetSingleton(out GameTime gameTime))
				return;

			var state = gameWorld.GetComponentData<RhythmEngineLocalState>(LocalEngine);
			if (state.CurrentBeat >= 0 && state.NewBeatTick == gameTime.Frame) AudioPlayerUtility.Play(audioPlayer);
		}
	}
}