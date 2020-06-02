using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Audio;
using GameHost.Core.Audio;
using GameHost.Core.Bindables;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.HostSerialization;
using GameHost.Injection.Dependency;
using GameHost.IO;
using Microsoft.Extensions.Logging;
using PataNext.Module.Presentation.RhythmEngine;
using PataNext.Module.RhythmEngine;
using PataponGameHost.RhythmEngine.Components;
using ZLogger;

namespace PataNext.Module.Presentation.BGM.Directors
{
	public class BgmDefaultDirectorCommandSystem : BgmDirectorySystemBase<BgmDefaultDirector, BgmDefaultSamplesLoader>
	{
		private readonly Dictionary<string, ComboBasedOutput> commandComboBasedOutputs;
		private readonly Bindable<ResourceHandle<AudioResource>> onEnterFever;
		private readonly Bindable<ResourceHandle<AudioResource>> onFeverLost;

		private EntitySet                 commandSet;
		private CurrentRhythmEngineSystem currentRhythmEngineSystem;
		private LoadAudioResourceSystem   loadAudio;

		private PresentationWorld presentation;
		private CustomModule module;

		private ILogger logger;

		private Entity audioPlayer;

		public BgmDefaultDirectorCommandSystem(WorldCollection collection) : base(collection)
		{
			commandComboBasedOutputs = new Dictionary<string, ComboBasedOutput>();

			onEnterFever = new Bindable<ResourceHandle<AudioResource>>();
			onFeverLost  = new Bindable<ResourceHandle<AudioResource>>();

			DependencyResolver.Add(() => ref presentation);
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref currentRhythmEngineSystem);
			DependencyResolver.Add(() => ref module);
			DependencyResolver.Add(() => ref logger);

			audioPlayer = collection.Mgr.CreateEntity();
			AudioPlayerUtility.Initialize<FlatAudioPlayerComponent>(audioPlayer);
		}

		protected override async void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			commandSet = presentation.World.GetEntities()
			                         .With<RhythmCommandDefinition>()
			                         .AsSet();

			IStorage storage = new StorageCollection {module.DllStorage, module.Storage.Value};
			storage = await storage.GetOrCreateDirectoryAsync("Sounds/RhythmEngine/Effects/");

			onEnterFever.Default = loadAudio.Start("voice_fever.wav", storage);
			onFeverLost.Default = loadAudio.Start("fever_lost.wav", storage);
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

			var incomingCommandBindable = Director.IncomingCommand;
			if (incomingCommandBindable.Value.CommandId != information.NextCommandId
			    || incomingCommandBindable.Value.Start != information.CommandStartTime)
			{
				incomingCommandBindable.Value = new BgmDefaultDirector.IncomingCommandData
				{
					CommandId = information.NextCommandId,
					Start     = information.CommandStartTime,
					End       = information.CommandEndTime
				};

				if (!string.IsNullOrEmpty(information.NextCommandId)
				    && information.CommandStartTime > TimeSpan.Zero
				    && commandComboBasedOutputs.TryGetValue(information.NextCommandId, out var output))
				{
					var comboSettings = information.Entity.Get<GameCombo.Settings>();
					var comboState    = information.Entity.Get<GameCombo.State>();
					var key           = "normal";
					if (comboSettings.CanEnterFever(comboState.Count, comboState.Score))
						key = "fever";
					else if (comboState.Score > 1)
						key = "prefever";

					var doFeverShout = false;
					if (key.Equals("fever"))
					{
						doFeverShout           = !Director.IsFever.Value;
						Director.IsFever.Value = true;
					}
					else
					{
						Director.IsFever.Value = false;
					}

					ResourceHandle<AudioResource> resourceHandle = default;
					if (doFeverShout)
					{
						resourceHandle = onEnterFever.Value.IsLoaded ? onEnterFever.Value : onEnterFever.Default;
					}
					else if (output.Map.TryGetValue(key, out var resourceMap)
					         && resourceMap.TryGetValue(Director.GetNextCycle(information.NextCommandId, key), out var resource))
					{
						resourceHandle = resource;
					}

					if (resourceHandle.Entity != default && resourceHandle.IsLoaded)
					{
						AudioPlayerUtility.SetResource(audioPlayer, resourceHandle);
						AudioPlayerUtility.PlayDelayed(audioPlayer, information.CommandStartTime - information.Elapsed);
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

			if (onEnterFever.Value.Entity == default)
			{
				try
				{
					var bFile = Loader.GetFile(new BFileOnEnterFeverSoundDescription());
					if (bFile is BgmDefaultSamplesLoader.SingleFile singleFile)
						onEnterFever.Value = loadAudio.Start(singleFile.File);
				}
				catch (Exception ex)
				{
					if (!thrownException.Contains(nameof(BFileOnEnterFeverSoundDescription)))
					{
						logger.ZLogError("No EnterFever sample found for BGM '{0}'", currentRhythmEngineSystem.Information.ActiveBgmId);
						thrownException.Add(nameof(BFileOnEnterFeverSoundDescription));
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