using System;
using GameHost.Core;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace PataNext.Export.Desktop.Visual
{
	public record SidebarMenuEntry(Texture Icon, string Name, Func<Screen> ScreenFactory)
	{
	}
	
	public class SidebarMenuTabControl : TabControl<SidebarMenuEntry>
	{
		private readonly float shearing, shearingPos;
		
		public SidebarMenuTabControl(float shearing, float shearingPos)
		{
			this.shearing    = shearing;
			this.shearingPos = shearingPos;
		}
		
		protected override Dropdown<SidebarMenuEntry> CreateDropdown()
		{
			return null;
		}
		
		protected override TabFillFlowContainer CreateTabFlow() => new()
		{
			RelativeSizeAxes = Axes.Y,
			AutoSizeAxes     = Axes.X,
			Direction        = FillDirection.Horizontal,
			
			Origin = Anchor.CentreRight,
			Anchor = Anchor.CentreRight
		};

		protected override TabItem<SidebarMenuEntry>  CreateTabItem(SidebarMenuEntry value)
		{
			return new SidebarMenuTabButton(value, shearing, shearingPos);
		}
	}

	public class SidebarMenuTabButton : TabItem<SidebarMenuEntry>
	{
		public readonly Bindable<SidebarMenuEntry> Current = new();

		private readonly float shearing, shearingPos;
		
		public SidebarMenuTabButton(SidebarMenuEntry value, float shearing, float shearingPos) : base(value)
		{
			Current.Value = value;

			this.shearing    = shearing;
			this.shearingPos = shearingPos;
			
			AutoSizeAxes     = Axes.X;
			RelativeSizeAxes = Axes.Y;

			AddRange(new Drawable[]
			{
				new Container
				{
					RelativeSizeAxes = Axes.Y,
					Size             = new(50, 1),

					Children = new Drawable[]
					{
						backgroundBox = new()
						{
							RelativeSizeAxes = Axes.Both,
							Size             = new(1, 1),
							
							Colour = Colour4.Black,
							Alpha  = 0.25f,

							Shear    = new(-shearing, 0),
							Position = new(-shearingPos, 0)
						},
						// Logo
						iconSprite = new()
						{
							FillAspectRatio = 1,
							FillMode        = FillMode.Fit,

							RelativeSizeAxes = Axes.Both,
							Size             = new(0.6f, 0.6f),
							Position         = new(-shearingPos / 1.5f, 0),

							Anchor = Anchor.Centre,
							Origin = Anchor.Centre,

							Texture = Value.Icon,
							Alpha   = 0.75f,

							Colour = Colour4.White
						},
						// Outline
						outlineBox = new Container
						{
							RelativeSizeAxes = Axes.Both,
							Size             = new(1, 1),
							
							Masking = true,
							BorderThickness = 4,
							BorderColour = Colour4.FromHex("b81c31"),
							CornerRadius = 1,

							Shear    = new(-shearing, 0),
							Position = new(-shearingPos, 0),
							
							Alpha = 0,
							
							Child = new Box {Size = new (1), RelativeSizeAxes = Axes.Both, Alpha = 0.01f}
						}
					}
				}
			});
		}

		private Box       backgroundBox;
		private Sprite    iconSprite;
		private Container outlineBox;
		
		protected override void OnActivated()
		{
			backgroundBox.FadeTo(0.8f);
			iconSprite.FadeTo(1, 100f);
			outlineBox.FadeTo(1, 100f);

			Console.WriteLine("active!");
		}

		protected override void OnDeactivated()
		{
			backgroundBox.FadeTo(0.25f);
			iconSprite.FadeTo(0.75f, 100f);
			outlineBox.FadeTo(0, 100f);
		}

		protected override bool OnHover(HoverEvent e)
		{
			if (Active.Value)
				return base.OnHover(e);
			
			iconSprite.FadeTo(1, 200f);
			backgroundBox.FadeTo(0.7f, 100f);
			
			return base.OnHover(e);
		}

		protected override void OnHoverLost(HoverLostEvent e)
		{
			if (Active.Value)
				return;
			
			iconSprite.FadeTo(0.75f, 100f);
			backgroundBox.FadeTo(0.25f, 100f);
			
			base.OnHoverLost(e);
		}
	}
}