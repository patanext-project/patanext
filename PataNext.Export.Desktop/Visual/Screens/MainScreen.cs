using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osuTK;
using PataNext.Export.Desktop.Visual.Overlays;
using PataNext.Export.Desktop.Visual.Screens.Section;
using PataNext.Export.Desktop.Visual.Screens.Toolbar;

namespace PataNext.Export.Desktop.Visual.Screens
{
	public class MainScreen : Screen
	{
		private GhMenuTabControl sectionContainer;

		public ScreenStack ScreenStack;
		public SpriteText  Notification;

		[BackgroundDependencyLoader]
		private void load(NotificationOverlay notificationOverlay)
		{
			AddRangeInternal(new[]
			{
				new DrawSizePreservingFillContainer
				{
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					Children = new Drawable[]
					{
						new Box
						{
							RelativeSizeAxes = Axes.Both,
							Size             = Vector2.One,
							Colour           = ColourInfo.GradientVertical(new Colour4(35, 16, 37, 255), new Colour4(20, 33, 82, 255))
						},
						new DrawSizePreservingFillContainer
						{
							Position         = new Vector2(0, 0),
							RelativeSizeAxes = Axes.Both,
							Origin           = Anchor.BottomCentre,
							Anchor           = Anchor.BottomCentre,
							Size             = new Vector2(1f, 0.875f),
							Child            = ScreenStack = new ScreenStack()
						},
						notificationOverlay.With(d =>
						{
							d.GetToolbarHeight = () => 50;
							d.Anchor           = Anchor.TopRight;
							d.Origin           = Anchor.TopRight;
						}),
						new Container
						{
							RelativeSizeAxes = Axes.X,
							Origin           = Anchor.TopCentre,
							Anchor           = Anchor.TopCentre,
							Size             = new Vector2(1, 60),
							Padding = new MarginPadding {Horizontal = 10},
							Position = new Vector2(0, 10),
							Masking = false,
							Children = new Drawable[]
							{
								new Container
								{
									RelativeSizeAxes = Axes.Both,
									Size             = Vector2.One,
									Masking          = true,
									CornerRadius = 13f,
									EdgeEffect = new EdgeEffectParameters
									{
										Radius    = 25,
										Roundness = 25
									},
									Children = new []
									{
										new Box
										{
											RelativeSizeAxes = Axes.Both,
											Size             = Vector2.One,
											Colour           = Colour4.Black.MultiplyAlpha(0.25f)
										},
									}
								},
								new SpriteText
								{
									Anchor   = Anchor.CentreLeft,
									Origin   = Anchor.CentreLeft,
									Position = new Vector2(20, -11),
									Text     = "PataNext",
									Font     = FontUsage.Default.With(size: 30),
									Colour   = Colour4.White,
								},
								new SpriteText
								{
									Anchor   = Anchor.CentreLeft,
									Origin   = Anchor.CentreLeft,
									Position = new Vector2(20, 18),
									Text     = "2020.17.12.2250",
									Font     = FontUsage.Default.With(size: 15),
									Colour   = Colour4.White.MultiplyAlpha(0.25f),
								},
								new SpriteText
								{
									Anchor   = Anchor.CentreLeft,
									Origin   = Anchor.CentreLeft,
									Position = new Vector2(20, 6),
									Text     = "GAMEHOST",
									Font     = FontUsage.Default.With(size: 17),
									Colour   = Colour4.White.MultiplyAlpha(0.5f),
								},
								sectionContainer = new GhMenuTabControl
								{
									Position         = new Vector2(130, 0),
									RelativeSizeAxes = Axes.Both
								},
								new FillFlowContainer
								{
									Anchor           = Anchor.TopRight,
									Origin           = Anchor.TopRight,
									Direction        = FillDirection.Horizontal,
									RelativeSizeAxes = Axes.Y,
									AutoSizeAxes     = Axes.X,
									Margin = new MarginPadding {Right = 10},
									Children = new Drawable[]
									{
										new ToolbarNotificationButton()
									}
								}
							}
						}
					}
				}
			});

			sectionContainer.AddItem(new GhMenuEntry(() => new GameSectionScreen(), "Game", "Game Home page", FontAwesome.Solid.Gamepad));
			sectionContainer.AddItem(new GhMenuEntry(() => new ClientSectionScreen(), "Client", "Launch game clients", FontAwesome.Solid.User));
			sectionContainer.AddItem(new GhMenuEntry(() => new ClientSectionScreen(), "Worlds", "World data debug", FontAwesome.Solid.Globe));
			sectionContainer.AddItem(new GhMenuEntry(() => new ClientSectionScreen(), "Files", "GameHost Configuration files", FontAwesome.Solid.Wrench));

			sectionContainer.Current.BindValueChanged(ev =>
			{
				if (ScreenStack.CurrentScreen != null)
					ScreenStack.Exit();
				if (ev.NewValue != null)
					ScreenStack.Push(ev.NewValue.Create());
			}, true);
		}
	}
}