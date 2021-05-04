using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform.Windows;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osuTK;
using PataNext.Export.Desktop.Visual;
using PataNext.Export.Desktop.Visual.Dependencies;
using PataNext.Export.Desktop.Visual.Screens;
using PataNext.Export.Desktop.Visual.Screens.Section;

namespace PataNext.Export.Desktop.Tests.Visual
{
	public class LauncherScene : TestScene
	{
		public Bindable<int> bindable = new(2);

		private ClientSectionScreen screen;

		private DependencyContainer dependencies;

		protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
		{
			return dependencies = new(base.CreateChildDependencies(parent));
		}

		public LauncherScene()
		{
			//var t = typeof(ClientSectionScreen);

			/*Add(new Box
			{
				Anchor = Anchor.TopCentre,
				Shear = new(0.5f, 0),
				Size   = new Vector2(120, 60)
			});*/

			/*RelativeSizeAxes = Axes.None;
			Size             = new(1920, 1080);*/
		}

		[Resolved]
		private TextureStore textures { get; set; }

		[BackgroundDependencyLoader]
		private void load()
		{
			Sidebar        sidebar;
			HomeLogoVisual logo;

			ScreenStack screenStack;

			dependencies.CacheAs<ICurrentVersion>(new CurrentVersion()
			{
				Current = {Value = "2021.04.15"}
			});
			dependencies.CacheAs<IUpdatePatchDownload>(new UpdatePatchDownload(Scheduler)
			{

			});
			dependencies.CacheAs<IGlobalNotifications>(new GlobalNotifications());

			Add(new DrawSizePreservingFillContainer()
			{
				/*Strategy        = DrawSizePreservationStrategy.Minimum,
				FillAspectRatio = 16 / 9f,

				FillMode = FillMode.Fit,*/

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

					/*new DrawSizePreservingFillContainer
					{
						RelativeSizeAxes = Axes.X,
						Size             = new(1, 500f),

						Position = new(0, 50),

						Origin = Anchor.BottomCentre,
						Anchor = Anchor.BottomCentre,

						FillAspectRatio = 16f / 9f,
						FillMode        = FillMode.Stretch,

						Children = new Drawable[]
						{
							new NotificationPopup
							{
								FillAspectRatio = 906f / 89f,
								FillMode        = FillMode.Fit,

								Origin = Anchor.Centre,
								Anchor = Anchor.Centre,

								RelativeSizeAxes = Axes.Both,
								Size             = new(0.47f, 0.1f),

								Position = new(0, 65),

								Font = new FontUsage("ar_cena", size: 21.5f),

								Title = "Information",
								Text  = "Update \"2021.04.15b\" is available!"
							},

							playBarControl = new ProgressPlayBarControl
							{
								FillAspectRatio = 1303f / 125f,
								FillMode        = FillMode.Fit,

								RelativeSizeAxes = Axes.Both,
								Size             = new(0.68f, 0.1425f),

								Origin = Anchor.Centre,
								Anchor = Anchor.Centre,
							},
						}
					},*/

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

			sidebar.Menu.AddItem(new(textures.Get("sidebar_settings"), "Settings", () => new()));
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

			/*n.Clicked += () =>
			{
				Logger.Log("launch update!", LoggingTarget.Information, LogLevel.Important);
			};*/

			AddToggleStep("IsUpdate", b => Dependencies.Get<IUpdatePatchDownload>().IsUpdating.Value                  = b);
			AddSliderStep("UpdateProgress", 0, 1, 0f, f => Dependencies.Get<IUpdatePatchDownload>().Progress.Value = f);
			AddStep("Add Update", () =>
			{
				var notifications = Dependencies.Get<IGlobalNotifications>();

				var updateNotification = new NotificationPopup
				{
					Title = "Information",
					Text  = "Update Available!",
					Icon  = textures.Get("popup_update")
				};

				updateNotification.Action = () =>
				{
					var updatePatch = Dependencies.Get<IUpdatePatchDownload>();
					updatePatch.StartDownload();
					
					notifications.Remove(updateNotification);
				};

				notifications.Push(updateNotification);
			});
		}
	}

	class CurrentVersion : ICurrentVersion
	{
		public Bindable<string> Current { get; } = new();
	}

	class UpdatePatchDownload : IUpdatePatchDownload
	{
		public Bindable<string> Version        { get; } = new();
		public BindableBool     IsUpdating     { get; } = new();
		public BindableBool     RequireRestart { get; } = new();
		public BindableFloat    Progress       { get; } = new();

		private Scheduler scheduler;

		public UpdatePatchDownload(Scheduler scheduler)
		{
			this.scheduler = scheduler;
		}

		public void StartDownload()
		{
			IsUpdating.Value = true;
			Progress.Value   = 0f;
			scheduler.AddDelayed(() => { Progress.Value = 0.25f; }, 1000);
			scheduler.AddDelayed(() => { Progress.Value = 0.5f; }, 1500);
			scheduler.AddDelayed(() => { Progress.Value = 0.75f; }, 2000);
			scheduler.AddDelayed(() =>
			{
				Progress.Value       = 1f; 
				IsUpdating.Value     = false;
				RequireRestart.Value = true;
			}, 2500);
		}

		public void UpdateAndRestart()
		{
			Console.WriteLine("update and restart!");
		}
	}

	class GlobalNotifications : IGlobalNotifications
	{
		private List<NotificationBase> notifications = new();

		public IReadOnlyList<NotificationBase> GetAll() => notifications;

		public void Push(NotificationBase notification)
		{
			notifications.Add(notification);
			OnNotificationAdded?.Invoke(notification);
		}

		public void ClearAll()
		{
			foreach (var notification in notifications)
				OnNotificationRemoved?.Invoke(notification);
			notifications.Clear();
		}

		public void Clear(Type type)
		{
			foreach (var notification in notifications.Where(notification => notification.GetType().IsSubclassOf(type)))
				OnNotificationRemoved?.Invoke(notification);
			notifications.RemoveAll(n => n.GetType().IsSubclassOf(type));
		}

		public void Remove(NotificationBase notification)
		{
			if (notifications.Remove(notification))
				OnNotificationRemoved?.Invoke(notification);
		}

		public event Action<NotificationBase> OnNotificationAdded;
		public event Action<NotificationBase> OnNotificationRemoved;
	}
}