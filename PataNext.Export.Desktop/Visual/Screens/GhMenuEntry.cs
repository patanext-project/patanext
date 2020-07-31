using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;

namespace PataNext.Export.Desktop.Visual.Screens
{
	public class GhMenuEntry
	{
		public Func<Screen>    Screen { get; }
		public string    Main   { get; }
		public string    Sub    { get; }
		public IconUsage Icon   { get; }

		public GhMenuEntry(Func<Screen> screen, string main, string sub, IconUsage icon)
		{
			Screen = screen;
			Main   = main;
			Sub    = sub;
			Icon   = icon;
		}

		public Screen Create()
		{
			return Screen();
		}
	}
}