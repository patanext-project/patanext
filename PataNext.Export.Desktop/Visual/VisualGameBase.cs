using System;
using osu.Framework.Allocation;
using osu.Framework.IO.Stores;

namespace PataNext.Export.Desktop.Visual
{
	public class VisualGameBase : osu.Framework.Game
	{
		[BackgroundDependencyLoader]
		private void load()
		{
			Resources.AddStore(new DllResourceStore(typeof(VisualGameBase).Assembly));
			Resources.AddStore(new DllResourceStore(typeof(PataNext.Game.Client.Resources.Module).Assembly));
			
			AddFont(Resources, @"Fonts/ar_cena");
			AddFont(Resources, @"Fonts/mojipon");
			AddFont(Resources, @"Fonts/roboto");
		}
	}
}