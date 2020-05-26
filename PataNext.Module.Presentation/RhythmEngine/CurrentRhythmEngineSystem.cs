using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Bindables;
using GameHost.Core.Ecs;
using GameHost.Entities;
using GameHost.HostSerialization;
using PataNext.Module.RhythmEngine;
using PataponGameHost.RhythmEngine.Components;
using RevolutionSnapshot.Core.ECS;

namespace PataNext.Module.Presentation.RhythmEngine
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	public class CurrentRhythmEngineSystem : AppSystem
	{
		public readonly Bindable<Entity> CurrentEntity;
		public readonly Bindable<string> CurrentBgmId;

		public RhythmEngineInformation Information => information;

		private PresentationWorld       presentation;
		private RhythmEngineInformation information;

		public CurrentRhythmEngineSystem(WorldCollection collection) : base(collection)
		{
			CurrentEntity = new Bindable<Entity>();
			CurrentBgmId  = new Bindable<string>();

			DependencyResolver.Add(() => ref presentation);
		}

		private EntitySet engineSet;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			engineSet = presentation.World.GetEntities()
			                        .With<RhythmEngineController>()
			                        .AsSet();
		}

		public override bool CanUpdate() => base.CanUpdate() && engineSet.Count > 0;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var engineEntity = engineSet.GetEntities()[0];

			CurrentEntity.Value = engineEntity;
			CurrentBgmId.Value  = "topkek";

			// compute engine data...
			information = default;

			information.Entity      = engineEntity;
			information.ActiveBgmId = "topkek";
			information.Elapsed     = engineEntity.Get<RhythmEngineLocalState>().Elapsed;

			if (engineEntity.TryGet(out RhythmEngineExecutingCommand executingCommand))
			{
				information.NextCommandId = null;
				if (executingCommand.CommandTarget != default
				    && executingCommand.CommandTarget.TryGet(out RhythmCommandDefinition definition))
					information.NextCommandId = definition.Identifier;

				var settings = engineEntity.Get<RhythmEngineSettings>();
				information.NextCommandStartTime = executingCommand.ActivationBeatStart * settings.BeatInterval;
				information.NextCommandEndTime   = executingCommand.ActivationBeatEnd * settings.BeatInterval;
			}
		}
	}

	public struct RhythmEngineInformation
	{
		public Entity Entity;
		public string ActiveBgmId;

		public TimeSpan Elapsed;

		public string   NextCommandId;
		public TimeSpan NextCommandStartTime;
		public TimeSpan NextCommandEndTime;
	}
}