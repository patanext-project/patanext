using System;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace PataNext.Game.Updater
{
	public abstract class GameUpdater : CompositeDrawable
	{
		protected override void LoadComplete()
		{
			base.LoadComplete();
			
			Schedule(() => Task.Run(CheckForUpdate));
		}

		public abstract Task CheckForUpdate();
	}
}