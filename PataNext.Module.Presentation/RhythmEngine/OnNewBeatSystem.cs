using System;
using System.Collections.Generic;
using System.Threading;
using DefaultEcs;
using DryIoc;
using GameHost;
using GameHost.Applications;
using GameHost.Audio;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Modding;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.IO;
using PataNext.Module.Simulation.RhythmEngine;

namespace PataNext.Module.Presentation.RhythmEngine
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	public class OnNewBeatSystem : AppSystem
	{
		private Instance runningInstance;
		private GameSimulationThreadingClient client;
		private LoadAudioResourceSystem loadAudio;
		private CModule module;
		public OnNewBeatSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref runningInstance);
			DependencyResolver.Add(() => ref client);
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref module);
		}
		
		private ResourceHandle<AudioResource> newBeatSound;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			newBeatSound = loadAudio.Start("on_new_beat.ogg", module.Storage.Value);
		}

		protected override void OnUpdate()
		{
			using (client.SynchronizeThread(out var host))
			{
				if (!host.MappedWorldCollection.TryGetValue(runningInstance, out var hostWorld))
					return;
				
				if (hostWorld.Mgr.Get<RhythmEngineOnNewBeat>().Length > 0)
					Console.WriteLine("on new beat!");
			}
		}
	}
}