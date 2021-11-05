using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;

namespace PataNext.Export.Desktop.Visual
{
	public class LauncherMainScene : Screen
	{
		[Resolved]
		private TextureStore textures { get; set; }

		private Sidebar        sidebar;
		private HomeLogoVisual logo;

		private ScreenStack screenStack;

		[BackgroundDependencyLoader]
		private void load(Storage storage)
		{
			AddInternal(new DrawSizePreservingFillContainer()
			{
				Children = new Drawable[]
				{
					new Box
					{
						Size             = Vector2.One,
						RelativeSizeAxes = Axes.Both,
						Colour           = Colour4.FromHex("be5a7f"),
					},
					new Sprite
					{
						Size             = Vector2.One,
						RelativeSizeAxes = Axes.Both,
						Colour           = Colour4.White,

						Scale    = new(1.03f),
						Position = new(0, -31f),

						Origin   = Anchor.Centre,
						Anchor   = Anchor.Centre,
						FillMode = FillMode.Fill,

						Texture = textures.Get("patanext_background_2")
					},

					new DrawSizePreservingFillContainer
					{
						RelativeSizeAxes = Axes.Both,
						Size             = new Vector2(0.97f, 0.8f),

						Origin = Anchor.TopCentre,
						Anchor = Anchor.TopCentre,

						RelativePositionAxes   = Axes.Both,
						RelativeAnchorPosition = new Vector2(0.5f, 0.2f),

						Child = screenStack = new()
						{
							Size             = new(1),
							RelativeSizeAxes = Axes.Both
						}
					},

					new Container()
					{
						Origin = Anchor.TopCentre,
						Anchor = Anchor.TopCentre,

						RelativeSizeAxes = Axes.Both,
						Size             = new(1, 0.25f),

						Children = new Drawable[]
						{
							sidebar = new Sidebar()
							{
								Origin  = Anchor.CentreLeft,
								Anchor  = Anchor.CentreLeft,
								Padding = new() {Left = 16},

								RelativeSizeAxes = Axes.X,
								Size             = new(0.38f, 43f),
							},
							logo = new HomeLogoVisual(textures.Get("patanext_logo"), textures.Get("patanext_logo_blur"))
							{
								Origin = Anchor.Centre,
								Anchor = Anchor.Centre,

								Size     = new Vector2(358, 218),
								Position = new Vector2(0, -20)
							},
							new Sidebar()
							{
								Origin  = Anchor.CentreRight,
								Anchor  = Anchor.CentreRight,
								Padding = new() {Right = 16},

								RelativeSizeAxes = Axes.X,
								Size             = new(0.38f, 43f),

								Flip = true,
							}
						}
					}
				}
			});

			initStuff();
		}

		private void initStuff()
		{
			sidebar.Add(new SidebarAccountDropdown
			{
				Origin = Anchor.CentreLeft,
				Anchor = Anchor.CentreLeft,
				
				Size = new(250, 1),
				RelativeSizeAxes = Axes.Y
			});

			sidebar.Menu.AddItem(new(textures.Get("sidebar_settings"), "Settings", () => new HomeSettingsScreen()));
			sidebar.Menu.AddItem(new(textures.Get("sidebar_changelogs"), "Changelogs", () =>
			{
				var screen = new HomeChangelogScreen();
				screen.Control.Set("2021.04.15", new()
				{
					{
						"2021.04.15", new[]
						{
							"#Launcher",
							"+ New launcher visual overhaul",
							"+ Inputs Settings Interface",
							"#Online",
							"+ Connection to MasterServer",
						}
					},
					{
						"2021.03.10", new[]
						{
							"+ Some random new stuff added"
						}
					},
					{
						"2021.03.09", new[]
						{
							"+ New launcher visual overhaul",
							"+ Connection to MasterServer",
							"+ Inputs Settings Interface"
						}
					},
					{
						"2021.03.08", new[]
						{
							"+ Some random new stuff added"
						}
					},
					{
						"2021.03.07", new[]
						{
							"+ New launcher visual overhaul",
							"+ Connection to MasterServer",
							"+ Inputs Settings Interface"
						}
					},
					{
						"2021.03.06", new[]
						{
							"+ Some random new stuff added"
						}
					}
				});

				return screen;
			}));

			sidebar.Menu.SelectFirstTabByDefault = false;

			sidebar.Menu.Current.SetDefault();

			sidebar.Menu.Current.BindValueChanged(ev =>
			{
				if (screenStack.CurrentScreen is not null)
					screenStack.Exit();

				if (ev.NewValue is { } entry)
				{
					// todo: some other stuff 

					logo.Active.Value = false;

					screenStack.Push(ev.NewValue.ScreenFactory());
				}
				else
				{
					logo.Active.Value = true;

					var homePlay = new HomePlayScreen();
					screenStack.Push(homePlay);
				}
			}, true);

			logo.Action = () => { sidebar.Menu.Current.SetDefault(); };
		}
	}
}