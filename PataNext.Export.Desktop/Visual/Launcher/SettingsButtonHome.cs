using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;

namespace PataNext.Export.Desktop.Visual
{
	public class SettingsButtonHome : SettingsButtonBase
	{
		private SpriteText titleSprite;
		private Sprite     iconSprite;

		public override LocalisableString Title
		{
			get => titleSprite.Text;
			set => titleSprite.Text = value;
		}

		public override Texture Icon
		{
			get => iconSprite.Texture;
			set => iconSprite.Texture = value;
		}

		private Box backgroundBox;
		public SettingsButtonHome()
		{
			Size = new(1, 50);
			
			titleSprite = new()
			{
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				
				Padding = new() {Left = 20},
				
				Font = new("ar_cena", 30),
				Colour = Colour4.FromRGBA(0xffedd2ff)
			};
			iconSprite = new();

			Masking      = true;
			CornerRadius = 8;
			AddRange(new Drawable[]
			{
				backgroundBox = new Box
				{
					Size = new(1),
					RelativeSizeAxes = Axes.Both,
					
					//Colour = Colour4.FromRGBA(0x25110aff).Lighten(4f)
					Colour = Colour4.FromRGBA(0xffedd2ff).Darken(0.2f)
				},
				titleSprite 
			});

			OnHoverLost(default);
		}

		protected override bool OnHover(HoverEvent e)
		{
			backgroundBox.FadeColour(Colour4.FromRGBA(0x25110aff));
			titleSprite.FadeColour(Colour4.FromRGBA(0xffedd2ff));
			
			return base.OnHover(e);
		}

		protected override void OnHoverLost(HoverLostEvent e)
		{
			backgroundBox.FadeColour(Colour4.FromRGBA(0xffedd2ff).Darken(0.2f));
			titleSprite.FadeColour(Colour4.FromRGBA(0x25110aff));			
			
			base.OnHoverLost(e);
		}
	}
}