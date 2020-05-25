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
	public class BgmDirectorySystemBase<T> : AppSystem
		where T : BgmDirectorBase
	{
		protected T Director;
		
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
			&& (Director = directorSet.GetEntities()[0].Get<BgmDirectorBase>() as T) != null;
	}
}