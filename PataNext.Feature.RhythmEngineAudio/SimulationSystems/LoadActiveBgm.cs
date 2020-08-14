using System;
using System.Linq;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Native.Char;
using GameHost.Simulation.Application;
using GameHost.Worlds;
using PataNext.Feature.RhythmEngineAudio.BGM;
using PataNext.Game.BGM;

namespace PataNext.Simulation.Client.Systems
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class LoadActiveBgmSystem : PresentationRhythmEngineSystemBase
	{
		private IScheduler  scheduler;
		private GlobalWorld globalWorld;

		private CharBuffer64 currentLoadedBgm;
		private bool         isBgmLoaded;

		public LoadActiveBgmSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref globalWorld);
		}

		protected override void OnUpdatePass()
		{
			if (currentLoadedBgm.Span.SequenceEqual(LocalInformation.ActiveBgmId.Span) && isBgmLoaded)
				return;
			
			currentLoadedBgm = LocalInformation.ActiveBgmId;
			if (currentLoadedBgm.GetLength() == 0)
				return;

			isBgmLoaded = true;

			// We use a double scheduler strategy.
			// - First schedule from the client app (main thread) to get the requested BGM file.
			// - Then once it's found, schedule from the client app to our app with the BGM file (and load it).
			// This should be 101% thread safe.
			globalWorld.Scheduler.Schedule(() =>
			{
				var file = (from ent in globalWorld.World
				            where ent.Has<BgmFile>()
				            select ent.Get<BgmFile>())
					.FirstOrDefault(bmgFile => bmgFile.Description.Id.AsSpan().SequenceEqual(currentLoadedBgm.Span));
				
				//Console.WriteLine($"File exist for '{currentLoadedBgm}' ? {file != null}");
				if (file == null)
					return;
				scheduler.Schedule(LoadBgm, file, default);
			}, default);
		}

		private void LoadBgm(BgmFile file)
		{
			// since the description has been already computed, there is no need to await the result
			var director = BgmDirector.Create(file).Result;
			var entity   = World.Mgr.CreateEntity();
			entity.Set(director);

			isBgmLoaded = true;
		}
	}
}