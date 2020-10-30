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
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Passes;
using PataNext.Module.Simulation.Resources;
using PataNext.Simulation.Client.Systems;
using PataNext.Simulation.Mixed.Components.GamePlay.Abilities;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using ZLogger;

namespace PataNext.Feature.RhythmEngineAudio.BGM.Directors
{
	public class BgmDefaultDirectorCommandSystem : BgmDirectorySystemBase<BgmDefaultDirector, BgmDefaultSamplesLoader>
	{
		private readonly Dictionary<CharBuffer64, ComboBasedOutput>    commandComboBasedOutputs;
		private readonly Bindable<ResourceHandle<AudioResource>>       onEnterFever;
		private readonly Bindable<ResourceHandle<AudioResource>>       onFeverLost;
		private readonly Bindable<List<ResourceHandle<AudioResource>>> onHeroModeCombo;

		private Entity audioPlayer;

		private EntitySet               commandSet;
		private AbilityHeroVoiceManager heroVoiceManager;
		private LoadAudioResourceSystem loadAudio;

		private ILogger logger;

		private CustomModule module;

		private readonly HashSet<CharBuffer64> thrownException = new HashSet<CharBuffer64>();

		public BgmDefaultDirectorCommandSystem(WorldCollection collection) : base(collection)
		{
			commandComboBasedOutputs = new Dictionary<CharBuffer64, ComboBasedOutput>();
			onHeroModeCombo          = new Bindable<List<ResourceHandle<AudioResource>>>();

			onEnterFever = new Bindable<ResourceHandle<AudioResource>>();
			onFeverLost  = new Bindable<ResourceHandle<AudioResource>>();

			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref heroVoiceManager);
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

			onHeroModeCombo.Default = new List<ResourceHandle<AudioResource>>();

			var heroModeComboStorage = await storage.GetOrCreateDirectoryAsync("HeroMode/Combo/");
			foreach (var file in heroModeComboStorage.GetFilesAsync("return*.wav").Result)
			{
				var resource = loadAudio.Load(file);

				var index = file.Name[6];
				if (index < onHeroModeCombo.Default.Count)
					onHeroModeCombo.Default.Insert(index, resource);
				else
					onHeroModeCombo.Default.Add(resource);
			}

			audioPlayer = World.Mgr.CreateEntity();
			AudioPlayerUtility.Initialize(audioPlayer, new StandardAudioPlayerComponent());
		}

		public override bool CanUpdate()
		{
			var canUpdate = base.CanUpdate() && LocalEngine != default;
			if (!canUpdate && World.DefaultSystemCollection.ExecutingRegister is IRhythmEngineSimulationPass.RegisterPass)
			{
				thrownException.Clear();

				commandComboBasedOutputs.Clear();
				onEnterFever.Value = default;
				onFeverLost.Value  = default;
			}

			return canUpdate;
		}

		protected override void OnUpdatePass()
		{
			if (!CanUpdate())
				return;

			loadFiles();

			var isHeroMode              = false;
			var heroModeCommandResource = default(ResourceHandle<AudioResource>);
			if (TryGetComponentData(LocalEngine, out Relative<PlayerDescription> relativePlayer))
			{
				CameraState cameraState = default, localCamState = default;
				if (TryGetComponentData(relativePlayer.Target, out ServerCameraState serverCameraState))
					cameraState                                                                                         = serverCameraState.Data;
				else if (TryGetComponentData(relativePlayer.Target, out LocalCameraState localCameraState)) cameraState = localCameraState.Data;

				if ((int) localCamState.Mode > (int) cameraState.Mode)
					cameraState = localCamState;

				if (GameWorld.Contains(cameraState.Target) && TryGetComponentData(cameraState.Target, out OwnerActiveAbility activeAbility)
				                                           && activeAbility.Incoming != default
				                                           && TryGetComponentData(activeAbility.Incoming, out AbilityActivation abilityActivation)
				                                           && TryGetComponentData(activeAbility.Incoming, out AbilityState abilityState)
				                                           && abilityActivation.Type == EAbilityActivationType.HeroMode)
				{
					isHeroMode = true;
					if (TryGetComponentData(activeAbility.Incoming, out NamedAbilityId namedAbilityId))
						heroModeCommandResource = heroVoiceManager.GetResource(namedAbilityId.Value.ToString());
				}
			}

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

						Director.HeroModeCombo.Value = 0;
					}
					else if (isHeroMode)
					{
						if (heroModeCommandResource.IsLoaded)
							resourceHandle = heroModeCommandResource;
						else Director.HeroModeCombo.Value++;

						if (Director.HeroModeCombo.Value > 0)
						{
							var comboList = onHeroModeCombo.Value ?? onHeroModeCombo.Default;
							resourceHandle = comboList[Director.HeroModeCombo.Value % comboList.Count];
						}

						Director.HeroModeCombo.Value++;
					}
					else if (output.Map.TryGetValue(key, out var resourceMap)
					         && resourceMap.TryGetValue(Director.GetNextCycle(LocalInformation.NextCommandStr, key), out var resource))
					{
						resourceHandle = resource;

						Director.HeroModeCombo.Value = 0;
					}

					if (resourceHandle.Entity != default && resourceHandle.IsLoaded)
					{
						AudioPlayerUtility.SetResource(audioPlayer, resourceHandle);
						AudioPlayerUtility.PlayDelayed(audioPlayer, LocalInformation.CommandStartTime - LocalInformation.Elapsed);
					}
				}
				else
				{
					AudioPlayerUtility.Stop(audioPlayer);
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

		private void loadFiles()
		{
			foreach (var commandEntity in GameWorld.QueryEntityWith(stackalloc[]
			{
				GameWorld.AsComponentType<RhythmCommandResource>()
			}))
			{
				var cmd = GameWorld.GetComponentData<RhythmCommandIdentifier>(commandEntity).Value;
				if (cmd.GetLength() == 0)
					continue;

				if (!commandComboBasedOutputs.TryGetValue(cmd, out var output))
					commandComboBasedOutputs[cmd] = output = new ComboBasedOutput();

				if (output.Source == null)
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
				try
				{
					var bFile = Loader.GetFile(new BFileOnEnterFeverSoundDescription());
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