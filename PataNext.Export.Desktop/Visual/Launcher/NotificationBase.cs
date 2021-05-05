using System;
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

		public Notification Source { get; }

		public NotificationBase(Notification notification)
		{
			Source = notification;
		}
	}

	public class Notification
	{
		public LocalisableString Title  { get; set; }
		public LocalisableString Text   { get; set; }
		public Texture           Icon   { get; set; }
		public Action            Action { get; set; }

		public virtual NotificationBase ProvideDrawable()
		{
			return null;
		}
	}
}