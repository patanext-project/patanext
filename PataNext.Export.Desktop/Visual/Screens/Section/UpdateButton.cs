using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Colour;
using osuTK.Graphics;

namespace PataNext.Export.Desktop.Visual.Screens.Section
{
	public class UpdateButton : GameSectionScreen.KButton
	{
		public UpdateButton()
		{
			BackgroundColour = Color4.GhostWhite.Opacity(0.75f);
			SpriteText.Colour = ColourInfo.SingleColour(Color4.Black);
		}
	}
}