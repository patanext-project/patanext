using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;

namespace PataNext.Export.Desktop.Visual.Overlays
{
	public class ProgressCompletionNotification : SimpleNotification
	{
		public ProgressCompletionNotification()
		{
			Icon = FontAwesome.Solid.Check;
		}

		[BackgroundDependencyLoader]
		private void load()
		{
			IconBackgound.Colour = ColourInfo.GradientVertical(Colour4.DarkGreen, Colour4.LightGreen);
		}
	}
}