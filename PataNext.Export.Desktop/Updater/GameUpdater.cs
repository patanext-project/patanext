using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using PataNext.Export.Desktop.Visual.Overlays;

namespace PataNext.Game.Updater
{
	public abstract class GameUpdater : CompositeDrawable
	{
		[Resolved]
		protected NotificationOverlay Notifications { get; private set; }
		
		protected override void LoadComplete()
		{
			base.LoadComplete();
			
			Schedule(() => Task.Run(CheckForUpdate));
		}

		public abstract Task CheckForUpdate();
	}
}