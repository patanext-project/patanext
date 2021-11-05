using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

namespace PataNext.Export.Desktop.Visual
{
	public class NotificationPopup : NotificationBase
	{
		private Box backgroundBox;

		private SpriteText title = new()
		{
			Origin = Anchor.TopLeft,
			Font   = new("ar_cena", size: 21.5f),

			Text   = "Information",
			Colour = Colour4.FromRGBA(0xd53535ff)
		};

		private SpriteText text = new()
		{
			Origin = Anchor.TopLeft,
			Font   = new("ar_cena", size: 21.5f),

			Text   = "Update \"2021.04.15b\" is available!",
			Colour = Colour4.FromRGBA(0x25110aff)
		};

		public override LocalisableString Title
		{
			get => title.Text;
			set => title.Text = value;
		}

		public override LocalisableString Text
		{
			get => text.Text;
			set => text.Text = value;
		}

		private FontUsage font;

		public FontUsage Font
		{
			get => font;
			set
			{
				font = value;

				title.Font = font;
				text.Font  = font;
			}
		}

		private Sprite iconSprite = new()
		{
			FillMode = FillMode.Fill,
			Size = new(0.15f, 1),
			RelativeSizeAxes = Axes.Both,
			
			Anchor = Anchor.CentreRight,
			Origin = Anchor.CentreRight,
			
			Position = new (10, 0),
			
			Colour = Colour4.FromRGBA(0x25110aff)
		};

		public override Texture Icon
		{
			get => iconSprite.Texture;
			set => iconSprite.Texture = value;
		}

		protected override void LoadComplete()
		{
			base.LoadComplete();

			Add(new Container()
			{
				RelativeSizeAxes = Axes.Both,
				Child = backgroundBox = new()
				{
					Size             = Vector2.One,
					RelativeSizeAxes = Axes.Both,
					Colour           = Colour4.FromRGBA(0xffedd2ff)
				},
				BorderColour    = SRGBColour.FromVector(new(0, 0, 0, 0.5f)),
				BorderThickness = 5f,
				CornerRadius    = 8,
				Masking         = true
			});
			Add(new FillFlowContainer()
			{
				Padding = new(5) {Left = 12.5f},
				Spacing = new(0, -2),
				Children = new[]
				{
					title,
					text,
				}
			});
			Add(new Container
			{
				Masking = true,
				
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre,
				
				Margin = new () {Right = 18},

				RelativeSizeAxes = Axes.Both,
				Size = new Vector2(1, 0.7f),
				
				Child = iconSprite
			});
		}

		protected override bool OnHover(HoverEvent e)
		{
			backgroundBox.FadeTo(0.9f, 200, Easing.OutCubic);
			iconSprite.ScaleTo(1.1f, 200, Easing.OutCubic);
			iconSprite.MoveToX(0, 200, Easing.OutCubic);

			return base.OnHover(e);
		}

		protected override void OnHoverLost(HoverLostEvent e)
		{
			backgroundBox.FadeTo(1, 200, Easing.InBack);
			iconSprite.ScaleTo(1f, 200, Easing.InCubic);
			iconSprite.MoveToX(8, 200, Easing.InCubic);

			base.OnHoverLost(e);
		}

		protected override bool OnMouseDown(MouseDownEvent e)
		{
			this.ScaleTo(0.95f, 100, Easing.InCirc);
			this.RotateTo(0.1f, 100, Easing.InCirc);

			return base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseUpEvent e)
		{
			this.ScaleTo(1, 400, Easing.OutElastic);
			this.RotateTo(0, 100, Easing.OutCirc);

			base.OnMouseUp(e);
		}

		protected override bool OnClick(ClickEvent e)
		{
			this.FadeTo(0, 250, Easing.None);
			this.ScaleTo(new Vector2(0.98f), 50, Easing.In)
			    .Then(o => o.ScaleTo(new Vector2(1.05f), 100, Easing.Out));
			this.RotateTo(0.3f, 300, Easing.OutCirc);

			Expire();

			return base.OnClick(e);
		}

		public NotificationPopup(Notification notification) : base(notification)
		{
		}
	}
}