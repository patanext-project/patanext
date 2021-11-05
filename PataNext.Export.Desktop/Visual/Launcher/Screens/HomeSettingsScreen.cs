using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK;
using PataNext.Export.Desktop.Visual.Overlays;
using PataNext.Export.Desktop.Visual.SettingsScreen;

namespace PataNext.Export.Desktop.Visual
{
	public class HomeSettingsScreen : Screen
	{
		public readonly ScreenStack ScreenStack = new()
		{
			Origin = Anchor.TopRight,
			Anchor = Anchor.TopRight,

			RelativeSizeAxes = Axes.Both,
			Size             = new(0.95f, 1),
		};

		public readonly SettingsControl Control = new()
		{
			Origin = Anchor.TopCentre,
			Anchor = Anchor.TopCentre,

			Size             = new(SettingsControl.WIDTH, 1),
			RelativeSizeAxes = Axes.Y,

			Margin = new() {Top = 60},
		};
		
		private readonly Container backContainer;

		private void goBack()
		{
			ScreenStack.Exit();
		}

		public HomeSettingsScreen()
		{
			Masking      = true;
			CornerRadius = 8;

			/*mainFlow = new AlwaysUpdateFillFlowContainer<Drawable>
			{
				Size             = new(WIDTH, 800),
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes     = Axes.Y,
				
				Direction = FillDirection.Vertical,
				
				Anchor = Anchor.TopCentre,
				Origin = Anchor.TopCentre,
				
				Spacing = new(0, 30)
			};*/

			AddRangeInternal(new Drawable[]
			{
				new Box {Size = new(1), RelativeSizeAxes = Axes.Both, Colour = Colour4.FromRGBA(0xffedd2ff)},
				new Container
				{
					Size             = new(1, 50),
					RelativeSizeAxes = Axes.X,

					Children = new Drawable[]
					{
						new SpriteText
						{
							Origin = Anchor.Centre, Anchor = Anchor.Centre,

							Text   = "Settings",
							Font   = new("mojipon"),
							Colour = Colour4.FromRGBA(0x25110aff)
						}
					}
				},
				Control,
				new Container
				{
					Size = new(1),
					RelativeSizeAxes = Axes.Both,
					
					Padding = new() {Top = 60},
					
					Child = ScreenStack
				},
				backContainer = new()
				{
					RelativeSizeAxes = Axes.Both,
					Origin           = Anchor.CentreLeft,
					Anchor           = Anchor.CentreLeft,
			
					Size  = new (0.05f, 1f),
					Scale = new (0, 1),
			
					Children = new Drawable[]
					{
						new BasicButton()
						{
							BackgroundColour = Colour4.FromRGBA(0x25110aff),
					
							Size             = new Vector2(1),
							RelativeSizeAxes = Axes.Both,
							Action           = () => goBack()
						},
						new Box()
						{
							RelativeSizeAxes = Axes.Both,
							Size             = new(0.15f, 1),
					
							Origin = Anchor.CentreRight,
							Anchor = Anchor.CentreRight,
					
							Colour = Colour4.Black.Opacity(0.5f)
						},
						new SpriteIcon
						{
							Icon             = FontAwesome.Solid.Backward,
							Size             = new(0.4f),
							RelativeSizeAxes = Axes.Both,
							Colour           = Colour4.Black.Opacity(0.5f),
					
							Origin = Anchor.Centre,
							Anchor = Anchor.Centre,
					
							X = -0,
							Y = 5
						},
						new SpriteIcon
						{
							Icon             = FontAwesome.Solid.Backward,
							Size             = new(0.4f),
							RelativeSizeAxes = Axes.Both,
							Colour           = Colour4.FromRGBA(0xffedd2ff),
					
							Origin = Anchor.Centre,
							Anchor = Anchor.Centre,
					
							X = -5
						}
					}
				}
			});
			
			Control.AddButton("Launcher", new SettingsButtonHome {Title = "Updates", Action = () => ScreenStack.Push(new SettingsScreenUpdate())});
			Control.AddButton("Launcher", new SettingsButtonHome {Title = "Directory"});
			Control.AddButton("Game", new SettingsButtonHome {Title     = "Inputs", Action = () => ScreenStack.Push(new SettingsScreenInput())});
			/*Control.AddButton("Game", new SettingsButtonHome {Title     = "Rhythm Engine"});
			Control.AddButton("Game", new SettingsButtonHome {Title     = "Networking"});
			Control.AddButton("Discord", new SettingsButtonHome {Title  = "Rich Presence"});
			Control.AddButton("Discord", new SettingsButtonHome {Title  = "MasterServer Auto-Connect"});*/

			ScreenStack.ScreenPushed += (screen, newScreen) =>
			{
				Control.Alpha       = 0;
				backContainer.ScaleTo(new Vector2(1, 1), 100);
			};
			ScreenStack.ScreenExited += (screen, newScreen) =>
			{
				Control.Alpha       = 1;
				backContainer.ScaleTo(new Vector2(0, 1), 100);
			};
		}
	}
}