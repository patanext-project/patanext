using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Bindables;
using GameHost.Core.Ecs;
using GameHost.HostSerialization;
using PataNext.Module.RhythmEngine;
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
			information.Entity      = engineEntity;
			information.ActiveBgmId = "topkek";
		}
	}

	public struct RhythmEngineInformation
	{
		public Entity Entity;
		public string ActiveBgmId;
	}
}