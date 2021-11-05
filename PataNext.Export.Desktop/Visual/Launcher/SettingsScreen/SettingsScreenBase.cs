using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;

namespace PataNext.Export.Desktop.Visual.SettingsScreen
{
	public class SettingsScreenBase : Screen
	{
		public const float WIDTH = 600;
		
		protected SettingsControl Control;
		
		public SettingsScreenBase()
		{
			Padding = new() {Horizontal = 40};
			
			/*AddInternal(mainFlow);*/
			AddInternal(Control = new()
			{
				Origin = Anchor.TopCentre,
				Anchor = Anchor.TopCentre,

				Size             = new(WIDTH, 1),
				RelativeSizeAxes = Axes.Y,
			});
		}
		
		protected void AddSetting(string name, Drawable drawable, float? height = null, string category = null)
		{
			drawable.Origin = Anchor.TopRight;
			drawable.Anchor = Anchor.TopRight;

			var container = new Container
			{
				Anchor = Anchor.TopCentre,
				Origin = Anchor.TopCentre,

				Size = new(WIDTH, height ?? Math.Max(drawable.Size.Y, 24)),
				Children = new[]
				{
					new SpriteText
					{
						Origin = Anchor.CentreLeft,
						Anchor = Anchor.CentreLeft,
						Text   = name,

						Font   = new("ar_cena", size: 24),
						Colour = Colour4.FromRGBA(0x25110aff),

						Size             = new(1),
						RelativeSizeAxes = Axes.Both
					},
					drawable
				}
			};

			if (category is null)
				Control.MainFlow.Add(container);
			else
			{
				Control.GetOrCreateCategoryFlow(category)
				       .Add(container);
			}
		}
	}
}