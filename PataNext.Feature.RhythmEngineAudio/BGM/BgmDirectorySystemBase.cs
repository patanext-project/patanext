using DefaultEcs;
using GameHost.Core.Ecs;
using PataNext.Simulation.Client.Systems;

namespace PataNext.Feature.RhythmEngineAudio.BGM
{
	//[UpdateAfter(typeof(LoadActiveBgmSystem))]
	public class BgmDirectorySystemBase<TDirector, TLoader> : PresentationRhythmEngineSystemBase
		where TDirector : BgmDirectorBase
		where TLoader : BgmSamplesLoaderBase
	{
		protected TDirector Director;

		private   EntitySet directorSet;
		protected TLoader   Loader;

		public BgmDirectorySystemBase(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnInit()
		{
			base.OnInit();

			directorSet = World.Mgr.GetEntities()
			                   .With<BgmDirectorBase>()
			                   .AsSet();
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate()
			       && directorSet.Count > 0
			       && (Director = directorSet.GetEntities()[0].Get<BgmDirectorBase>() as TDirector) != null
			       && (Loader = Director.Loader as TLoader) != null;
		}
	}
}