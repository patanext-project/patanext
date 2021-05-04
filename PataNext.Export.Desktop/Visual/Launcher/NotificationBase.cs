using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;

namespace PataNext.Export.Desktop.Visual
{
	public abstract class NotificationBase : Button
	{
		public abstract LocalisableString Title { get; set; }
		public abstract LocalisableString Text  { get; set; }
		public abstract Texture           Icon  { get; set; }
	}
}