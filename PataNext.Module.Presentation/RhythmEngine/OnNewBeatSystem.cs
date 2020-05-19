using System;
using System.Collections.Generic;
using GameHost;
using GameHost.Applications;
using GameHost.Audio;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Modding;
using GameHost.HostSerialization;
using GameHost.IO;
using PataNext.Module.Simulation.RhythmEngine;

namespace PataNext.Module.Presentation.RhythmEngine
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	public class OnNewBeatSystem : AppSystem
	{
		private PresentationHostWorld   presentationHost;
		private Instance                runningInstance;
		private LoadAudioResourceSystem loadAudio;
		private CModule                 module;

		public OnNewBeatSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref runningInstance);
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref presentationHost);
			DependencyResolver.Add(() => ref module);
		}

		private ResourceHandle<AudioResource> newBeatSound;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			newBeatSound = loadAudio.Start("on_new_beat.ogg", module.Storage.Value);
		}

		private readonly Query queryNewBeats = new Query {All = new[] {typeof(RhythmEngineOnNewBeat), typeof(RhythmEngineController)}};

		protected override void OnUpdate()
		{
			foreach (ref readonly var world in presentationHost.ActiveWorlds)
			{
				var hasBeenFound = false;
				foreach (var _ in world.QueryChunks(queryNewBeats))
				{
					hasBeenFound = true;
					break;
				}

				// There may be chance that the render client lagged, so we need to be sure that we only play one sound
				if (hasBeenFound)
				{
					Console.WriteLine("on new beat!");
					break;
				}
			}
		}
	}
}