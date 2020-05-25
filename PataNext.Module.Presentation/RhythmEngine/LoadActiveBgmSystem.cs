using System;
using System.Collections.Generic;
using System.Linq;
using GameHost.Applications;
using GameHost.Core.Bindables;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using PataNext.Module.Presentation.BGM;
using PataponGameHost.Storage;

namespace PataNext.Module.Presentation.RhythmEngine
{
	public class LoadActiveBgmSystem : AppSystem
	{
		private MainThreadClient client;
		private CurrentRhythmEngineSystem currentRhythmEngine;

		private IScheduler scheduler;
		
		public LoadActiveBgmSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref client);
			DependencyResolver.Add(() => ref currentRhythmEngine);
			DependencyResolver.Add(() => ref scheduler);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			currentRhythmEngine.CurrentBgmId.Subscribe((_, bgm) =>
			{
				if (string.IsNullOrEmpty(bgm))
					return;

				// We use a double scheduler strategy.
				// - First schedule from the client app (main thread) to get the requested BGM file.
				// - Then once it's found, schedule from the client app to our app with the BGM file (and load it).
				// This should be 101% thread safe.
				client.Listener.GetScheduler().Add(() =>
				{
					var clientWorld = client.Listener
					                        .WorldCollection
					                        .Mgr;
					
					var file = (from ent in clientWorld
					            where ent.Has<BgmFile>()
					            select ent.Get<BgmFile>())
						.FirstOrDefault(bmgFile => bmgFile.Description.Id == bgm);
					if (file == null)
						return;

					scheduler.Add(() => { LoadBgm(file); });
				});
			}, true);
		}

		private void LoadBgm(BgmFile file)
		{
			// since the description has been already computed, there is no need to await the result
			var director = BgmDirector.Create(file).Result;

			var entity = World.Mgr.CreateEntity();
			entity.Set(director);
		}
	}
}