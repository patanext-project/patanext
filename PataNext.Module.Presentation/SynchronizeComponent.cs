using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Entities;
using GameHost.HostSerialization;
using GameHost.HostSerialization.imp;
using PataNext.Module.RhythmEngine;
using PataponGameHost.Inputs;
using PataponGameHost.RhythmEngine.Components;

namespace PataNext.Module.Presentation
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	public class SynchronizeComponent : AppSystem
	{
		private PresentationHostWorld presentation;

		public SynchronizeComponent(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref presentation);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			presentation.Subscribe<WorldTime>();
			presentation.Subscribe<RhythmEngineController>();
			presentation.Subscribe<RhythmEngineSettings>();
			presentation.Subscribe<RhythmEngineLocalState>();
			presentation.Subscribe<RhythmEngineExecutingCommand>()
			            .AddTransformEntities();
			presentation.Subscribe(new CopyableComponentOperation<RhythmCommandDefinition>());
			presentation.Subscribe<RhythmEngineOnNewBeat>();
			presentation.Subscribe<PlayerInput>();
		}
	}
}