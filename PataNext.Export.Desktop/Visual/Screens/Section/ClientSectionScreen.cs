using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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
				Colour = Colour4.Black.MultiplyAlpha(0.5f),
			});
			AddInternal(new FillFlowContainer
			{
				RelativeSizeAxes = Axes.Both,
				Size             = Vector2.One,
				Children = new []
				{
					new SpriteText
					{
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,
						
						Text   = "This screen is currently in development!",

						Shadow       = true,
						Font         = FontUsage.Default.With(family: "OpenSans-Bold", size: 50),
						ShadowOffset = new Vector2(0.05f),
						ShadowColour = Colour4.Black.MultiplyAlpha(1f)
					}	
				}
			});
		}
	}
}