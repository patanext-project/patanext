using System;
using System.Collections.Generic;
using System.Linq;
using GameHost.Audio;
using GameHost.Core.Ecs;
using GameHost.IO;

namespace PataNext.Module.Presentation.BGM.Directors
{
	public class BgmDefaultDirectorCommandSystem : BgmDirectorySystemBase<BgmDefaultDirector>
	{
		public class ComboBasedOutput
		{
			public BgmDefaultDirector.ComboBasedCommand Source;

			public class InAudioResources : Dictionary<int, ResourceHandle<AudioResource>> {}
			
			public Dictionary<string, InAudioResources> Map = new Dictionary<string, InAudioResources>();
		}

		private LoadAudioResourceSystem loadAudio;

		public BgmDefaultDirectorCommandSystem(WorldCollection collection) : base(collection)
		{
			commandComboBasedOutputs = new Dictionary<string, ComboBasedOutput>();

			DependencyResolver.Add(() => ref loadAudio);
		}

		private Dictionary<string, ComboBasedOutput> commandComboBasedOutputs;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			foreach (var cmd in new[] {"march"})
			{
				if (!commandComboBasedOutputs.TryGetValue(cmd, out var output))
				{
					commandComboBasedOutputs[cmd] = output = new ComboBasedOutput();
				}

				if (output.Source == null)
				{
					output.Source = Director.GetCommand(cmd) as BgmDefaultDirector.ComboBasedCommand;
				}
				else
				{
					foreach (var (type, commandFiles) in output.Source.mappedFile)
					{
						if (!output.Map.TryGetValue(type, out var ar))
							output.Map[type] = ar = new ComboBasedOutput.InAudioResources();

						for (var i = 0; i < commandFiles.Count; i++)
						{
							var file = commandFiles[i];
							if (!ar.ContainsKey(i))
								ar[i] = loadAudio.Start(file);
						}
					}
				}
			}
		}
	}
}