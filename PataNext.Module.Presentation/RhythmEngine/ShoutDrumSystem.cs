using System;
using System.Collections.Generic;
using System.Linq;
using Collections.Pooled;
using GameHost.Applications;
using GameHost.Audio;
using GameHost.Core.Applications;
using GameHost.Core.Audio;
using GameHost.Core.Ecs;
using GameHost.HostSerialization;
using GameHost.IO;
using PataponGameHost.Inputs;

namespace PataNext.Module.Presentation.RhythmEngine
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	public class ShoutDrumSystem : AppSystem
	{
		private readonly Query playerQuery = new Query {All = new[] {typeof(PlayerInput)}};

		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureDrum =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private readonly PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>> audioOnPressureVoice =
			new PooledDictionary<int, PooledDictionary<int, ResourceHandle<AudioResource>>>();

		private LoadAudioResourceSystem loadAudio;
		private CustomModule            module;
		private PresentationHostWorld   presentation;

		public ShoutDrumSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref module);
			DependencyResolver.Add(() => ref presentation);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			var storage = new StorageCollection {module.DllStorage, module.Storage.Value}
			              .GetOrCreateDirectoryAsync("Sounds/Drums")
			              .Result;

			Console.WriteLine($"size on dll: {module.DllStorage.GetFilesAsync("*drum_1_0.ogg").Result.First().GetContentAsync().Result.Length}");
			Console.WriteLine($"size on int: {module.Storage.Value.GetFilesAsync("Sounds/Drums/drum_1_0.ogg").Result.First().GetContentAsync().Result.Length}");

			// TODO: should we have a configuration file for mapping the audio? (instead of hardcoding the mapping)
			for (var key = 1; key != 5; key++)
			{
				audioOnPressureDrum[key]  = new PooledDictionary<int, ResourceHandle<AudioResource>>();
				audioOnPressureVoice[key] = new PooledDictionary<int, ResourceHandle<AudioResource>>();

				for (var rank = 0; rank != 3; rank++)
				{
					audioOnPressureDrum[key][rank]  = loadAudio.Start($"drum_{key}_{rank}.ogg", storage);
					audioOnPressureVoice[key][rank] = loadAudio.Start($"voice_{key}_{rank}.wav", storage);
				}
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			foreach (ref readonly var world in presentation.ActiveWorlds)
			{
				if (!world.QueryEntities(playerQuery).TryGetFirst(out var playerEntity))
					continue;

				ref readonly var playerInput = ref playerEntity.GetComponent<PlayerInput>();
				for (var i = 0; i < playerInput.Actions.Length; i++)
				{
					if (!playerInput.Actions[i].WasPressed)
						continue;

					var play = World.Mgr.CreateEntity();
					play.Set(audioOnPressureDrum[i + 1][0].Result);
					play.Set(new AudioVolumeComponent(1));
					play.Set(new PlayFlatAudioComponent());
				}
			}
		}
	}
}