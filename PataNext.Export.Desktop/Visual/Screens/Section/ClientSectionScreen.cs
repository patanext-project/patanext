using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK;

namespace PataNext.Export.Desktop.Visual.Screens.Section
{
	public class ClientSectionScreen : Screen
	{
		[BackgroundDependencyLoader]
		private void load()
		{
			AddInternal(new Box
			{
				RelativeSizeAxes = Axes.Both,
				Size = Vector2.One,
				Colour = Colour4.White
			});
		}
	}
}