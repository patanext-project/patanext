using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
		
		public void Register(string id, string path)
		{
			// for now it only support one match type...
			foreach (Match match in Regex.Matches(path, "\\[(.*?)\\]"))
			{
				var splits = match.Value.Substring(1, match.Value.Length - 2).Split(',');
				if (splits.Length != 2)
					continue;

				var type = splits[0];
				switch (type)
				{
					case "module":
					{
						var moduleName = splits[1];
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
								var m       = path.Replace(match.Value, string.Empty);
								if (m.StartsWith('/') || m.StartsWith('\\'))
									m = m.Substring(1);
								
								var file    = storage.GetFilesAsync(m).Result.FirstOrDefault();
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

						return;
					}
				}
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