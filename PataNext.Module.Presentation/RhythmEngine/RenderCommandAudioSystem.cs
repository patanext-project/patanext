using System;
using GameHost.Applications;
using GameHost.Core;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;

namespace PataNext.Module.Presentation.RhythmEngine
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	[UpdateAfter(typeof(CurrentRhythmEngineSystem))]
	public class RenderCommandAudioSystem : AppSystem
	{
		private CurrentRhythmEngineSystem currentRhythmEngine;

		public RenderCommandAudioSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref currentRhythmEngine);
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate() && currentRhythmEngine.CurrentEntity.Value.IsAlive;
		}

		protected override void OnUpdate()
		{
			var information = currentRhythmEngine.Information;
		}
	}
}