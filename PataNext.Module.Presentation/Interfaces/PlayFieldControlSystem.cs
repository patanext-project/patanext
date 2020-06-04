using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Threading;
using GameHost.Entities;
using GameHost.HostSerialization;
using GameHost.UI.Noesis;
using Noesis;
using OpenToolkit.Windowing.Common;
using PataNext.Module.Presentation.RhythmEngine;
using PataNext.Module.RhythmEngine;
using PataponGameHost.Applications.MainThread;
using PataponGameHost.Inputs;
using PataponGameHost.RhythmEngine.Components;
using PataponGameHost.Storage;
using RevolutionSnapshot.Core;

namespace PataNext.Module.Presentation.Controls
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	[UpdateAfter(typeof(NoesisInitializationSystem))]
	public class PlayFieldControlSystem : AppSystem
	{
		private XamlFileLoader xamlFileLoader;
		private IScheduler     scheduler;

		private INativeWindow window;

		private PresentationWorld         presentationWorld;
		private CurrentRhythmEngineSystem currentRhythmEngineSystem;

		private IManagedWorldTime wt;

		public PlayFieldControlSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref window);
			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref xamlFileLoader);
			DependencyResolver.Add(() => ref presentationWorld);
			DependencyResolver.Add(() => ref currentRhythmEngineSystem);
			DependencyResolver.Add(() => ref wt);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			xamlFileLoader.SetTarget("Interfaces", "PlayFieldControl");
			xamlFileLoader.Xaml.Subscribe(OnXamlFound, true);
			
			AddDisposable(presentationWorld.World.SubscribeComponentAdded((in Entity entity, in RhythmEngineOnNewBeat next) =>
			{
				if (entity != currentRhythmEngineSystem.CurrentEntity.Value)
					return;

				isNewBeat = true;
			}));
			AddDisposable(presentationWorld.World.SubscribeComponentChanged((in Entity entity, in PlayerInput previous, in PlayerInput next) =>
			{
				playerInput = next;
				isNewPlayerInput = true;
			}));
		}

		private Entity entityView;
		private bool isNewBeat;
		private bool isNewPlayerInput;
		private PlayerInput playerInput;

		private void OnXamlFound(string previous, string next)
		{
			if (next == null)
				return;

			void addXaml()
			{
				var view = new NoesisOpenTkRenderer(window);
				view.ParseXaml(next);

				entityView = entityView.IsAlive ? entityView : World.Mgr.CreateEntity();
				if (entityView.Has<NoesisOpenTkRenderer>())
				{
					var oldRenderer = entityView.Get<NoesisOpenTkRenderer>();
					oldRenderer.Dispose();
				}

				entityView.Set(view);
				entityView.Set((PlayFieldControl) view.View.Content);
			}

			scheduler.Add(addXaml);
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate() && entityView.IsAlive;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			xamlFileLoader.Update();

			var information = currentRhythmEngineSystem.Information;
			switch (entityView.IsEnabled())
			{
				case true when information.Entity == default:
					entityView.Disable();
					break;
				case false when information.Entity != default:
					entityView.Enable();
					break;
			}

			if (!entityView.IsEnabled())
				return;

			foreach (ref var control in World.Mgr.Get<PlayFieldControl>())
			{
				if (!control.IsLoaded)
					continue;

				if (!(control.DataContext is PlayFieldControl.ViewModel view))
					continue;

				var phase = EPhase.NoCommand;
				if (information.Entity.TryGet(out GameCombo.State comboState)
				    && information.Entity.TryGet(out GameCombo.Settings comboSettings)
				    && comboSettings.CanEnterFever(comboState.Count, comboState.Score))
				{
					phase = EPhase.Fever;
				}

				if (information.Entity.TryGet(out GameCommandState commandState))
				{
					var elapsedMs = information.Elapsed.TotalMilliseconds;
					if (commandState.StartTimeMs < elapsedMs && elapsedMs < commandState.EndTimeMs)
						phase = EPhase.Command;
				}

				if (isNewBeat)
				{
					view.AccumulatedBeatTime    = TimeSpan.Zero;
					view.AccumulatedBeatTimeExp = TimeSpan.Zero;

					control.OnNewBeat(information.Elapsed, Colors.White);
				}
				else
				{
					if (phase == EPhase.Fever)
					{
						view.AccumulatedBeatTime += wt.Delta * 2;
					}
					else
					{
						view.AccumulatedBeatTime += wt.Delta * 2 + view.AccumulatedBeatTimeExp * 0.25;
					}

					view.AccumulatedBeatTimeExp += wt.Delta;
				}

				control.BeatImpulse.Opacity = MathHelper.Lerp(1, 0, Math.Clamp((float) view.AccumulatedBeatTime.TotalSeconds, 0f, 1f));
				switch (phase)
				{
					case EPhase.NoCommand:
						control.BeatImpulseColor = Color.FromScRgb(1, 0.75f, 0.75f, 0.75f);

						view.EnabledTrails[0] = true;
						view.EnabledTrails[1] = false;
						view.EnabledTrails[2] = false;
						break;
					case EPhase.Command:
						control.BeatImpulseColor = Color.FromScRgb(1, 0.5f, 0.5f, 0.5f);

						view.EnabledTrails[0] = true;
						view.EnabledTrails[1] = false;
						view.EnabledTrails[2] = true;
						break;
					case EPhase.Fever:
						control.BeatImpulseColor = Color.FromScRgb(1, 1, 0.86f, 0);

						view.EnabledTrails[0] = true;
						view.EnabledTrails[1] = true;
						view.EnabledTrails[2] = true;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (isNewPlayerInput)
				{
					for (var i = 0; i != playerInput.Actions.Length; i++)
					{
						var ac = playerInput.Actions[i];
						if (!ac.FrameUpdate)
							continue;

						if (ac.WasReleased && !ac.IsSliding)
							continue;

						i++;
						switch (i)
						{
							case RhythmKeys.Pata:
								control.OnNewPressure(information.Elapsed, Colors.DarkRed);
								break;
							case RhythmKeys.Pon:
								control.OnNewPressure(information.Elapsed, Colors.DarkBlue);
								break;
							case RhythmKeys.Don:
								control.OnNewPressure(information.Elapsed, Colors.Yellow);
								break;
							case RhythmKeys.Chaka:
								control.OnNewPressure(information.Elapsed, Colors.DarkGreen);
								break;
						}

						i--;
					}
				}

				control.BeatInterval = information.Entity.Get<RhythmEngineSettings>().BeatInterval;
				control.Elapsed      = information.Elapsed;
			}

			isNewBeat        = false;
			isNewPlayerInput = false;
		}

		enum EPhase
		{
			NoCommand,
			Command,
			Fever
		}
	}
}