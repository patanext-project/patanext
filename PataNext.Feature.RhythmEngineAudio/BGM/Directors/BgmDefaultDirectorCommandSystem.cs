using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Audio.Features;
using GameHost.Audio.Players;
using GameHost.Audio.Systems;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.IO;
using GameHost.Native.Char;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource.Components;
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Resources.Keys;
using ZLogger;


namespace PataNext.Feature.RhythmEngineAudio.BGM.Directors
{
	public class BgmDefaultDirectorCommandSystem : BgmDirectorySystemBase<BgmDefaultDirector, BgmDefaultSamplesLoader>
	{
		private readonly Dictionary<CharBuffer64, ComboBasedOutput> commandComboBasedOutputs;
		private readonly Bindable<ResourceHandle<AudioResource>>    onEnterFever;
		private readonly Bindable<ResourceHandle<AudioResource>>    onFeverLost;

		private EntitySet               commandSet;
		private LoadAudioResourceSystem loadAudio;

		private CustomModule module;

		private ILogger logger;

		private Entity audioPlayer;

		public BgmDefaultDirectorCommandSystem(WorldCollection collection) : base(collection)
		{
			commandComboBasedOutputs = new Dictionary<CharBuffer64, ComboBasedOutput>();

			onEnterFever = new Bindable<ResourceHandle<AudioResource>>();
			onFeverLost  = new Bindable<ResourceHandle<AudioResource>>();

			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref module);
			DependencyResolver.Add(() => ref logger);
		}

		protected override async void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			IStorage storage = new StorageCollection {module.DllStorage, module.Storage.Value};
			storage = await storage.GetOrCreateDirectoryAsync("Sounds/RhythmEngine/Effects/");

			onEnterFever.Default = loadAudio.Load("voice_fever.wav", storage);
			onFeverLost.Default  = loadAudio.Load("fever_lost.wav", storage);

			audioPlayer = World.Mgr.CreateEntity();
			AudioPlayerUtility.Initialize(audioPlayer, new StandardAudioPlayerComponent());
		}

		public override bool CanUpdate()
		{
			var canUpdate = base.CanUpdate() && LocalEngine != default;
			if (!canUpdate)
			{
				thrownException.Clear();

				commandComboBasedOutputs.Clear();
				onEnterFever.Value = default;
				onFeverLost.Value  = default;
			}

			return canUpdate;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			loadFiles();

			var comboSettings = GameWorld.GetComponentData<GameCombo.Settings>(LocalEngine);
			var comboState    = GameWorld.GetComponentData<GameCombo.State>(LocalEngine);
			
			var incomingCommandBindable = Director.IncomingCommand;
			if (incomingCommandBindable.Value.CommandId != LocalInformation.NextCommand
			    || incomingCommandBindable.Value.Start != LocalInformation.CommandStartTime)
			{
				incomingCommandBindable.Value = new BgmDefaultDirector.IncomingCommandData
				{
					CommandId = LocalInformation.NextCommand,
					Start     = LocalInformation.CommandStartTime,
					End       = LocalInformation.CommandEndTime
				};

				if (LocalInformation.NextCommand != default
				    && LocalInformation.CommandStartTime > TimeSpan.Zero
				    && commandComboBasedOutputs.TryGetValue(LocalInformation.NextCommandStr, out var output))
				{
					var key = "normal";
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
					         && resourceMap.TryGetValue(Director.GetNextCycle(LocalInformation.NextCommandStr, key), out var resource))
					{
						resourceHandle = resource;
					}

					if (resourceHandle.Entity != default && resourceHandle.IsLoaded)
					{
						AudioPlayerUtility.SetResource(audioPlayer, resourceHandle);
						AudioPlayerUtility.PlayDelayed(audioPlayer, LocalInformation.CommandStartTime - LocalInformation.Elapsed);
					}
				}
			}
			else
			{
				if (Director.IsFever.Value && !comboSettings.CanEnterFever(comboState.Count, comboState.Score))
				{
					Director.IsFever.Value = false;

					var resourceHandle = onFeverLost.Value.IsLoaded ? onFeverLost.Value : onFeverLost.Default;
					AudioPlayerUtility.SetResource(audioPlayer, resourceHandle);
					AudioPlayerUtility.Play(audioPlayer);
				}
			}
		}

		private HashSet<CharBuffer64> thrownException = new HashSet<CharBuffer64>();

		private void loadFiles()
		{
			foreach (var commandEntity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<GameResourceKey<RhythmCommandResourceKey>>()
			}))
			{
				var cmd = GameWorld.GetComponentData<GameResourceKey<RhythmCommandResourceKey>>(commandEntity).Value.Identifier;
				if (cmd.GetLength() == 0)
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
								ar[i] = loadAudio.Load(file);
						}
					}
			}

			if (onEnterFever.Value.Entity == default)
			{
				try
				{
					var bFile = Loader.GetFile(new BFileOnEnterFeverSoundDescription());
					Console.WriteLine(bFile);
					if (bFile is BgmDefaultSamplesLoader.SingleFile singleFile)
						onEnterFever.Value = loadAudio.Load(singleFile.File);
				}
				catch (Exception ex)
				{
					if (!thrownException.Contains(nameof(BFileOnEnterFeverSoundDescription)))
					{
						logger.ZLogError("No EnterFever sample found for BGM '{0}'", LocalInformation.ActiveBgmId);
						thrownException.Add(nameof(BFileOnEnterFeverSoundDescription));
					}
				}
			}
		}

		public class ComboBasedOutput
		{
			public Dictionary<string, InAudioResources>      Map = new Dictionary<string, InAudioResources>();
			public BgmDefaultSamplesLoader.ComboBasedCommand Source;

			public class InAudioResources : Dictionary<int, ResourceHandle<AudioResource>>
			{
			}
		}
	}
}