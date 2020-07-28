using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Audio.Features;
using GameHost.Audio.Players;
using GameHost.Audio.Systems;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.IO;
using GameHost.Native.Char;
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Game.RhythmEngine;
using ZLogger;


namespace PataNext.Feature.RhythmEngineAudio.BGM.Directors
{
	[UpdateAfter(typeof(BgmDefaultDirectorCommandSystem))]
	public class BgmDefaultDirectorSoundtrackSystem : BgmDirectorySystemBase<BgmDefaultDirector, BgmDefaultSamplesLoader>
	{
		private LoadAudioResourceSystem         loadAudio;
		private BgmDefaultDirectorCommandSystem commandSystem;

		private ILogger logger;

		private Entity audioPlayer;

		public BgmDefaultDirectorSoundtrackSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref commandSystem);
			DependencyResolver.Add(() => ref logger);

			mappedResources = new Dictionary<string, PooledList<ResourceHandle<AudioResource>>>();
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			audioPlayer = World.Mgr.CreateEntity();
			AudioPlayerUtility.Initialize(audioPlayer, new StandardAudioPlayerComponent());
		}

		public override bool CanUpdate()
		{
			var canUpdate = base.CanUpdate() && LocalEngine != default;
			if (!canUpdate)
			{
				mappedResources.Clear();

				BgmWasFever          = default;
				BgmFeverComboStart        = default;
				m_LastClip           = default;
				m_EndFeverEntranceAt = default;

				thrownException.Clear();
			}

			return canUpdate;
		}

		public const int SongBeatSize = 4;

		private bool                          BgmWasFever;
		private int                           BgmFeverComboStart;
		private ResourceHandle<AudioResource> m_LastClip;
		private int                           m_EndFeverEntranceAt;
		private int                           m_NextLoopTrigger;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			loadFiles();

			if (mappedResources.Count == 0)
				return;

			var beforeEntranceCount = 0;
			if (mappedResources.ContainsKey("before_entrance"))
				beforeEntranceCount = mappedResources["before_entrance"].Count;

			var state         = GameWorld.GetComponentData<RhythmEngineLocalState>(LocalEngine);
			var settings      = GameWorld.GetComponentData<RhythmEngineSettings>(LocalEngine);
			var comboSettings = GameWorld.GetComponentData<GameCombo.Settings>(LocalEngine);
			var comboState    = GameWorld.GetComponentData<GameCombo.State>(LocalEngine);

			if (state.CurrentBeat < 0)
				return;

			var targetAudio = m_LastClip;

			var activationBeat = RhythmEngineUtility.GetActivationBeat(state, settings);
			var flowBeat       = RhythmEngineUtility.GetFlowBeat(state, settings);

			(string type, int index) track = default;
			if (activationBeat >= beforeEntranceCount * SongBeatSize * 2 || comboState.Count > 0)
			{
				if (!comboSettings.CanEnterFever(comboState))
				{
					BgmWasFever   = false;
					BgmFeverComboStart = 0;

					var score = Math.Max((int) comboState.Score + Math.Max(comboState.Score >= 1.9 ? 1 + comboState.Count : 0, 0), comboState.Count);
					if (score < 0)
						score = 0;

					track = Director.GetNextTrack(false, false, score);
				}
				else
				{
					track = Director.GetNextTrack(true, true, 0);
					
					if (!BgmWasFever)
					{
						m_EndFeverEntranceAt = activationBeat + mappedResources["fever_entrance"].Count * SongBeatSize * 2;
						BgmWasFever          = true;

						BgmFeverComboStart = comboState.Count;
					}
					else if (m_EndFeverEntranceAt <= activationBeat)
					{
						track = Director.GetNextTrack(false, true, Math.Max(0, comboState.Count - BgmFeverComboStart - 1));
					}
				}
			}
			else
			{
				var commandLength = activationBeat != 0 ? activationBeat / SongBeatSize : 0;
				track = Director.GetNextTrack(true, false, commandLength);
			}

			if (string.IsNullOrEmpty(track.type))
			{
				logger.ZLogError("No Track Found");
				return;
			}

			try
			{
				targetAudio = mappedResources[track.type][track.index];
			}
			catch (Exception exception)
			{
				thrownException.Add(track.type);
				logger.ZLogError($"Error with TrackType={track.type} Idx={track.index}");
				return;
			}

			var cmdStartActivationBeat = RhythmEngineUtility.GetActivationBeat(LocalInformation.CommandStartTime, settings.BeatInterval);
			if (cmdStartActivationBeat > activationBeat)
				activationBeat = cmdStartActivationBeat - 1;

			var nextBeatDelay = (activationBeat + 1) * settings.BeatInterval - state.Elapsed;
			
			// Check if we should change clips or if we are requested to...
			var hasSwitched = false;
			if (m_LastClip != targetAudio) // switch audio if we are requested to
			{
				hasSwitched = Switch(targetAudio, nextBeatDelay);
				if (track.type == "before" && track.index == 0)
					m_NextLoopTrigger = activationBeat + SongBeatSize * 2;
				else
					m_NextLoopTrigger = -1;
			}

			var currentResource = AudioPlayerUtility.GetResource(audioPlayer);
			if (!hasSwitched && m_NextLoopTrigger > 0 && activationBeat >= m_NextLoopTrigger && currentResource.IsLoaded)
			{
				hasSwitched = Switch(targetAudio, nextBeatDelay);
				
				m_NextLoopTrigger = activationBeat + SongBeatSize * 2;
			}
			
			if (!hasSwitched && currentResource != default && AudioPlayerUtility.GetPlayTime(audioPlayer) + TimeSpan.FromSeconds(0.1) >= currentResource.Result.Length)
			{

			}
		}

		private bool Switch(ResourceHandle<AudioResource> targetAudio, TimeSpan delay)
		{
			var hasSwitched = false;

			m_LastClip = targetAudio;
			if (!targetAudio.IsLoaded)
			{
				logger.ZLogError("Resource {0} not loaded", targetAudio.Entity);
				AudioPlayerUtility.Stop(audioPlayer);

				m_LastClip = default;
			}
			else
			{
				AudioPlayerUtility.SetResource(audioPlayer, targetAudio);
				AudioPlayerUtility.PlayDelayed(audioPlayer, delay);

				hasSwitched = true;
			}

			return hasSwitched;
		}

		private HashSet<CharBuffer64> thrownException = new HashSet<CharBuffer64>();

		private void loadFiles()
		{
			if (!(Director.Loader.GetSoundtrack() is BgmDefaultSamplesLoader.SlicedSoundTrack slicedSoundTrack))
				return;

			foreach (var (identifier, files) in slicedSoundTrack.mappedFile)
			{
				if (mappedResources.ContainsKey(identifier))
					return;

				var list = new PooledList<ResourceHandle<AudioResource>>(files.Count);
				foreach (var file in files)
				{
					list.Add(loadAudio.Load(file));
				}

				mappedResources[identifier] = list;
			}
		}

		private Dictionary<string, PooledList<ResourceHandle<AudioResource>>> mappedResources;
	}
}