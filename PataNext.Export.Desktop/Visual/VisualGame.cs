using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;
using PataNext.Export.Desktop.Updater;

namespace PataNext.Export.Desktop.Visual
{
	public class VisualGame : osu.Framework.Game
	{
		private Box box;

		[BackgroundDependencyLoader]
		private void load()
		{
			Child = box = new Box
			{
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre,
				Colour = Color4.Orange,
				Size   = new Vector2(200),
			};
		}

		protected override void LoadComplete()
		{
			base.LoadComplete();

			box.Loop(b => b.RotateTo(0).RotateTo(360, 2500));
			
			LoadComponentAsync(new SquirrelUpdater(), Add);
			LoadComponentAsync(new GameHostApplicationRunner(), Add);
		}
	}
}