using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Audio;
using GameHost.Core.Applications;
using GameHost.Core.Audio;
using GameHost.Core.Ecs;
using GameHost.Core.Modding;
using GameHost.HostSerialization;
using GameHost.IO;
using PataNext.Module.RhythmEngine;

namespace PataNext.Module.Presentation.RhythmEngine
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	public class OnNewBeatSystem : AppSystem
	{
		private readonly Query                   queryNewBeats = new Query {All = new[] {typeof(RhythmEngineOnNewBeat), typeof(RhythmEngineController)}};
		private          LoadAudioResourceSystem loadAudio;
		private          CModule                 module;

		private ResourceHandle<AudioResource> newBeatSound;
		private PresentationHostWorld         presentationHost;

		public OnNewBeatSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref presentationHost);
			DependencyResolver.Add(() => ref module);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			newBeatSound = loadAudio.Start("on_new_beat.ogg", new StorageCollection {module.DllStorage, module.Storage.Value});
		}

		protected override void OnUpdate()
		{
			foreach (ref readonly var snapshot in presentationHost.ActiveWorlds)
			{
				if (!snapshot.QueryChunks(queryNewBeats).MoveNext())
					continue;

				var play = World.Mgr.CreateEntity();
				play.Set(newBeatSound.Result);
				play.Set(new AudioVolumeComponent(1));
				play.Set(new PlayFlatAudioComponent());

				// There may be chance that the render client lagged, so we need to be sure that we only play one sound
				break;
			}
		}
	}
}