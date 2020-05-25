using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.HostSerialization;
using PataNext.Module.RhythmEngine;
using PataponGameHost.Inputs;

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
			presentation.Subscribe<RhythmEngineController>();
			//presentation.Subscribe<RhythmEngineOnNewBeat, QueuedComponentOperation<RhythmEngineOnNewBeat>>(new QueuedComponentOperation<RhythmEngineOnNewBeat>());
			presentation.Subscribe<RhythmEngineOnNewBeat>();
			presentation.Subscribe<PlayerInput>();
		}
	}
}