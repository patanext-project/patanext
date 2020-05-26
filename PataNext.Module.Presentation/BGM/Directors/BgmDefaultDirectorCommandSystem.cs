using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Audio;
using GameHost.Core.Audio;
using GameHost.Core.Ecs;
using GameHost.HostSerialization;
using GameHost.IO;
using PataNext.Module.Presentation.RhythmEngine;
using PataNext.Module.RhythmEngine;

namespace PataNext.Module.Presentation.BGM.Directors
{
	public class BgmDefaultDirectorCommandSystem : BgmDirectorySystemBase<BgmDefaultDirector, BgmDefaultSamplesLoader>
	{
		private readonly Dictionary<string, ComboBasedOutput> commandComboBasedOutputs;

		private EntitySet                 commandSet;
		private CurrentRhythmEngineSystem currentRhythmEngineSystem;
		private LoadAudioResourceSystem   loadAudio;

		private PresentationWorld presentation;

		public BgmDefaultDirectorCommandSystem(WorldCollection collection) : base(collection)
		{
			commandComboBasedOutputs = new Dictionary<string, ComboBasedOutput>();

			DependencyResolver.Add(() => ref presentation);
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref currentRhythmEngineSystem);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			commandSet = presentation.World.GetEntities()
			                         .With<RhythmCommandDefinition>()
			                         .AsSet();
		}

		public override bool CanUpdate()
		{
			var canUpdate= base.CanUpdate() && currentRhythmEngineSystem.CurrentEntity != default;
			if (!canUpdate)
			{
				thrownException.Clear();
			}

			return canUpdate;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			loadFiles();

			var information = currentRhythmEngineSystem.Information;

			var incomingCommandBindable = Director.IncomingCommandBindable;
			if (incomingCommandBindable.Value.CommandId != information.NextCommandId
			    || incomingCommandBindable.Value.Start != information.CommandStartTime)
			{
				incomingCommandBindable.Value = new BgmDefaultDirector.IncomingCommand
				{
					CommandId = information.NextCommandId,
					Start     = information.CommandStartTime,
					End       = information.CommandEndTime
				};

				if (!string.IsNullOrEmpty(information.NextCommandId)
				    && information.CommandStartTime > TimeSpan.Zero
				    && commandComboBasedOutputs.TryGetValue(information.NextCommandId, out var output))
				{
					var key = "normal";
					if (output.Map.TryGetValue(key, out var resourceMap)
					    && resourceMap.TryGetValue(Director.GetNextCycle(information.NextCommandId, "normal"), out var resource))
					{
						var sound = World.Mgr.CreateEntity();
						sound.Set(resource.Result);
						sound.Set(new AudioVolumeComponent(1));
						sound.Set(new AudioDelayComponent(information.CommandStartTime - information.Elapsed));
						sound.Set(new PlayFlatAudioComponent());

						Console.WriteLine($"{information.CommandStartTime - information.Elapsed}");
					}
				}
			}
		}

		private HashSet<string> thrownException = new HashSet<string>();
		private void loadFiles()
		{
			foreach (ref readonly var commandEntity in commandSet.GetEntities())
			{
				var cmd = commandEntity.Get<RhythmCommandDefinition>().Identifier;
				if (string.IsNullOrEmpty(cmd))
					continue;

				if (!commandComboBasedOutputs.TryGetValue(cmd, out var output))
					commandComboBasedOutputs[cmd] = output = new ComboBasedOutput();

				if (output.Source == null)
				{
					try
					{
						output.Source = Loader.GetCommand(cmd) as BgmDefaultSamplesLoader.ComboBasedCommand;
					}
					catch (Exception ex)
					{
						if (thrownException.Contains(cmd))
							continue;

						thrownException.Add(cmd);
						Console.WriteLine($"Exception with command '{cmd}'\n{ex}");
					}
				}
				else
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

		public class ComboBasedOutput
		{
			public Dictionary<string, InAudioResources> Map = new Dictionary<string, InAudioResources>();
			public BgmDefaultSamplesLoader.ComboBasedCommand Source;

			public class InAudioResources : Dictionary<int, ResourceHandle<AudioResource>>
			{
			}
		}
	}
}