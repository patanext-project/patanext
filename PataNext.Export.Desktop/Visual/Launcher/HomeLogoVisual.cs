using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;

namespace PataNext.Export.Desktop.Visual
{
	public class HomeLogoVisual : Container
	{
		private Sprite    glowBox, logoBox;
		private HoverArea hoverArea;

		public readonly BindableBool Active = new();

		public Action Action
		{
			get => hoverArea.Action;
			set => hoverArea.Action = value;
		}

		public HomeLogoVisual(Texture texture, Texture glow)
		{
			Add(glowBox = new Sprite()
			{
				Origin           = Anchor.Centre,
				Anchor           = Anchor.Centre,
				RelativeSizeAxes = Axes.Both,

				Texture = glow,
			});
			Add(logoBox= new Sprite()
			{
				Origin           = Anchor.Centre,
				Anchor           = Anchor.Centre,
				RelativeSizeAxes = Axes.Both,

				Texture = texture
			});
			Add(hoverArea = new HoverArea()
			{
				Origin           = Anchor.Centre,
				Anchor           = Anchor.Centre,
				RelativeSizeAxes = Axes.Both,
				
				Scale = new Vector2(0.7f) 
			});
			
			Active.BindValueChanged(ev =>
			{
				glowBox.FadeTo(ev.NewValue ? 1 : 0, 100f);
				logoBox.FadeTo(ev.NewValue ? 1 : 0.8f, 100f);

				this.ScaleTo(ev.NewValue ? 1.05f : 0.95f, 100f);
			}, true);

			hoverArea.IsHover.BindValueChanged(ev =>
			{
				if (Active.Value)
					return;
				
				glowBox.FadeTo(ev.NewValue ? 0.5f : 0f, 200f);
				logoBox.FadeTo(ev.NewValue ? 1 : 0.8f, 100f);
				this.ScaleTo(ev.NewValue ? 0.98f : 0.95f, 200f);
			}, true);
		}

		class HoverArea : Button
		{
			public readonly BindableBool IsHover = new();

			protected override bool OnHover(HoverEvent e)
			{
				IsHover.Value = true;
				
				return base.OnHover(e);
			}

			protected override void OnHoverLost(HoverLostEvent e)
			{
				IsHover.Value = false;
				
				base.OnHoverLost(e);
			}
		}
	}
}