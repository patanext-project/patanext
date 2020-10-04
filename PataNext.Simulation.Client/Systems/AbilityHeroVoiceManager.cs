using System;
using System.Collections.Generic;
using System.Linq;
using GameHost.Audio.Players;
using GameHost.Audio.Systems;
using GameHost.Core.Ecs;
using GameHost.Core.Modules.Feature;
using GameHost.Core.Threading;
using GameHost.IO;
using GameHost.Worlds;

namespace PataNext.Simulation.Client.Systems
{
	public class AbilityHeroVoiceManager : AppSystem
	{
		private GlobalWorld             globalWorld;
		private LoadAudioResourceSystem loadAudio;

		private IScheduler scheduler;
		
		public AbilityHeroVoiceManager(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref globalWorld);
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref scheduler);
		}

		private readonly Dictionary<string, ResourceHandle<AudioResource>> audioMap = new Dictionary<string, ResourceHandle<AudioResource>>();

		private string ReplaceFirst(string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}

			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public void Register(string id, string path)
		{
			if (path.StartsWith("[module,"))
			{
				path = path.Remove(0, "[module,".Length + path.LastIndexOf("[module,", StringComparison.InvariantCulture));

				var moduleName = path.Substring(0, path.LastIndexOf(']'));
				path = path.Remove(path.IndexOf(']'), 1);
				path = ReplaceFirst(path, moduleName + "/", string.Empty);

				globalWorld.Scheduler.Schedule(() =>
				{
					if (!globalWorld.Collection.TryGet(out ModuleManager moduleManager))
						throw new Exception("ModuleManager not found in GlobalWorld?");

					var ghModule = moduleManager.GetModule(moduleName);
					if (ghModule == null)
						throw new Exception("No module named: " + moduleName);

					ghModule.Storage.Subscribe((_, localStorage) =>
					{
						if (localStorage == null)
							return;

						ghModule.Storage.UnsubscribeCurrent();

						var storage = new StorageCollection {localStorage, ghModule.DllStorage};
						var file    = storage.GetFilesAsync(path).Result.FirstOrDefault();
						if (file != null)
						{
							scheduler.Schedule(() => { RegisterFinalize(id, loadAudio.Load(file)); }, default);
						}
						else
						{
							Console.WriteLine("Error with path=" + path);
						}
					}, true);
				}, default);
			}
		}

		private void RegisterFinalize(string id, ResourceHandle<AudioResource> audio)
		{
			audioMap[id] = audio;
		}

		public ResourceHandle<AudioResource> GetResource(string id)
		{
			audioMap.TryGetValue(id, out var resourceHandle);
			return resourceHandle;
		}
	}
}