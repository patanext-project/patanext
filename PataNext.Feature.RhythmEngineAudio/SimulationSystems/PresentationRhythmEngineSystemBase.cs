using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Native.Char;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.RhythmEngine.Systems;
using PataNext.Module.Simulation.Passes;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Simulation.Client.Systems
{
	[UpdateAfter(typeof(ProcessEngineSystem))]
	[UpdateAfter(typeof(OnInputForRhythmEngine))]
	[UpdateAfter(typeof(ApplyCommandEngineSystem))]
	public class PresentationRhythmEngineSystemStart : AppSystem, IRhythmEngineSimulationPass
	{
		private GameWorld gameWorld;

		public PresentationRhythmEngineSystemStart(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref gameWorld);
		}

		public GameEntityHandle              LocalRhythmEngine { get; set; }
		public RhythmEngineInformation LocalInformation  { get; set; }

		public void OnRhythmEngineSimulationPass()
		{
			var playerEnumerator = gameWorld.QueryEntityWith(stackalloc[]
			{
				gameWorld.AsComponentType<PlayerDescription>(),
				gameWorld.AsComponentType<PlayerIsLocal>()
			});

			if (!playerEnumerator.TryGetFirst(out var localPlayerEntity))
				return;
			
			foreach (var entity in gameWorld.QueryEntityWith(stackalloc[]
			{
				gameWorld.AsComponentType<RhythmEngineDescription>(),
				gameWorld.AsComponentType<Relative<PlayerDescription>>()
			}))
			{
				if (gameWorld.GetComponentData<Relative<PlayerDescription>>(entity).Target != gameWorld.Safe(localPlayerEntity))
					continue;

				LocalRhythmEngine = entity;
				break;
			}

			var localInfo = new RhythmEngineInformation();
			if (LocalRhythmEngine != default)
			{
				localInfo.ActiveBgmId = "topkek";
				localInfo.Elapsed     = gameWorld.GetComponentData<RhythmEngineLocalState>(LocalRhythmEngine).Elapsed;
				if (gameWorld.HasComponent<RhythmEngineExecutingCommand>(LocalRhythmEngine))
				{
					var executingCommand = gameWorld.GetComponentData<RhythmEngineExecutingCommand>(LocalRhythmEngine);
					if (executingCommand.CommandTarget != default)
					{
						localInfo.NextCommand = executingCommand.CommandTarget;

						var key = gameWorld.GetComponentData<RhythmCommandIdentifier>(localInfo.NextCommand.Entity.Handle).Value;
						localInfo.NextCommandStr = key;
					}

					var settings = gameWorld.GetComponentData<RhythmEngineSettings>(LocalRhythmEngine);
					if (gameWorld.HasComponent<GameCommandState>(LocalRhythmEngine))
					{
						var commandState = gameWorld.GetComponentData<GameCommandState>(LocalRhythmEngine);
						localInfo.CommandStartTime = TimeSpan.FromMilliseconds(commandState.StartTimeMs);
						localInfo.CommandEndTime   = TimeSpan.FromMilliseconds(commandState.EndTimeMs);
					}
					else
					{
						localInfo.CommandStartTime = executingCommand.ActivationBeatStart * settings.BeatInterval;
						localInfo.CommandEndTime   = executingCommand.ActivationBeatEnd * settings.BeatInterval;
					}
				}
			}

			LocalInformation = localInfo;
		}

		public struct RhythmEngineInformation
		{
			public CharBuffer64 ActiveBgmId;

			public TimeSpan Elapsed;

			public GameResource<RhythmCommandResource> NextCommand;
			public CharBuffer64                        NextCommandStr;

			public TimeSpan CommandStartTime;
			public TimeSpan CommandEndTime;
		}
	}

	[RestrictToApplication(typeof(SimulationApplication))]
	[UpdateAfter(typeof(PresentationRhythmEngineSystemStart))]
	public abstract class PresentationRhythmEngineSystemBase : GameAppSystem, IPostUpdateSimulationPass
	{
		private PresentationRhythmEngineSystemStart start;

		protected PresentationRhythmEngineSystemBase(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref start);
		}

		public GameEntityHandle                                            LocalEngine      => start.LocalRhythmEngine;
		public PresentationRhythmEngineSystemStart.RhythmEngineInformation LocalInformation => start.LocalInformation;

		public void OnAfterSimulationUpdate()
		{
			OnUpdatePass();
		}

		protected abstract void OnUpdatePass();
	}
}