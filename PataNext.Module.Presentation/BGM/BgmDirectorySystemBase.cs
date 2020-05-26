using DefaultEcs;
using GameHost.Applications;
using GameHost.Core;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using PataNext.Module.Presentation.RhythmEngine;

namespace PataNext.Module.Presentation.BGM
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	[UpdateAfter(typeof(LoadActiveBgmSystem))]
	public class BgmDirectorySystemBase<TDirector, TLoader> : AppSystem
		where TDirector : BgmDirectorBase
		where TLoader : BgmSamplesLoaderBase
	{
		protected TDirector Director;
		protected TLoader   Loader;

		public BgmDirectorySystemBase(WorldCollection collection) : base(collection)
		{
		}

		private EntitySet directorSet;

		protected override void OnInit()
		{
			base.OnInit();

			directorSet = World.Mgr.GetEntities()
			                   .With<BgmDirectorBase>()
			                   .AsSet();
		}

		public override bool CanUpdate() =>
			base.CanUpdate()
			&& directorSet.Count > 0
			&& (Director = directorSet.GetEntities()[0].Get<BgmDirectorBase>() as TDirector) != null
			&& (Loader = Director.Loader as TLoader) != null;
	}
}