using System;
using GameHost.Core.Client;
using GameHost.Game;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osuTK;
using PataNext.Export.Desktop.Visual.Dependencies;

namespace PataNext.Export.Desktop.Visual
{
	public class HomePlayScreen : Screen
	{
		public readonly ProgressPlayBarControl ProgressPlayBar;

		private FillFlowContainer flow;

		[Resolved]
		private GameBootstrap gameBootstrap { get; set; }

		public HomePlayScreen()
		{
			AddRangeInternal(new Drawable[]
			{
				flow = new()
				{
					Margin = new() {Bottom = 30},

					Size             = new(1),
					RelativeSizeAxes = Axes.Both,

					Spacing   = new Vector2(0, 20),
					Direction = FillDirection.Vertical,

					Origin = Anchor.BottomCentre,
					Anchor = Anchor.BottomCentre,

					Children = new Drawable[]
					{
						ProgressPlayBar = new()
						{
							FillAspectRatio = 1303f / 125f,
							FillMode        = FillMode.Fit,

							RelativeSizeAxes = Axes.Both,
							Size             = new(0.68f, 0.1425f),

							Origin = Anchor.BottomCentre,
							Anchor = Anchor.BottomCentre,
						}
					}
				}
			});

			ProgressPlayBar.PlayAction = () =>
			{
				if (ProgressPlayBar.RequireRestart.Value && Dependencies.TryGet(out IPatchProvider patch))
					Scheduler.AddOnce(() => patch.UpdateAndRestart());
				else if (!ProgressPlayBar.RequireRestart.Value)
				{
					Console.WriteLine("launch game");

					var world = gameBootstrap.Global.World;
					foreach (var entity in world)
					{
						if (!entity.Has<ClientBootstrap>())
							continue;

						Console.WriteLine("create");
						var launch = world.CreateEntity();
						launch.Set(new LaunchClient(entity));
						break;
					}
				}
			};
		}

		public void PushPopup(string title, string description, Action action, Texture texture = null, Notification notification = null)
		{
			flow.Add(new NotificationPopup(notification)
			{
				FillAspectRatio = 906f / 89f,
				FillMode        = FillMode.Fit,

				Origin = Anchor.BottomCentre,
				Anchor = Anchor.BottomCentre,

				RelativeSizeAxes = Axes.Both,
				Size             = new(0.47f, 0.1f),

				Font = new("ar_cena", size: 21.5f),

				Title = title,
				Text  = description,
				Icon  = texture,
				
				Action = action
			});
		}

		[Resolved]
		private INotificationsProvider NotificationsProvider { get; set; }

		[BackgroundDependencyLoader]
		private void load(IPatchProvider patch, ICurrentVersion currentVersion)
		{
			patch.IsUpdating.BindValueChanged(ev => ProgressPlayBar.IsUpdating.Value   = ev.NewValue, true);
			patch.Progress.BindValueChanged(ev => ProgressPlayBar.UpdateProgress.Value = ev.NewValue, true);
			
			patch.RequireRestart.BindValueChanged(ev =>
			{
				ProgressPlayBar.RequireRestart.Value = ev.NewValue;
			}, true);
			
			currentVersion.Current.BindValueChanged(ev => ProgressPlayBar.CurrentVersion.Value = ev.NewValue, true);
			
			NotificationsProvider.OnNotificationAdded += addNotification;
			NotificationsProvider.OnNotificationRemoved += removeNotification;
			foreach (var notification in NotificationsProvider.GetAll())
			{
				addNotification(notification);
			}
		}

		private void removeNotification(Notification n)
		{
			Console.WriteLine("remove " + n.Title);
			foreach (var drawable in flow.Children)
			{
				if (drawable is NotificationPopup notificationDrawable && notificationDrawable.Source == n)
				{
					Schedule(() => flow.Remove(drawable));
				}
			}
		}

		private void addNotification(Notification n)
		{
			var         provided = n.ProvideDrawable();
			if (provided is { } and not NotificationPopup)
				return;

			if (provided == null)
			{
				PushPopup(n.Title.ToString(), n.Text.ToString(), n.Action, n.Icon, n);
				return;
			}

			provided.FillAspectRatio = 906f / 89f;
			provided.FillMode        = FillMode.Fit;
				
			provided.RelativeSizeAxes = Axes.Both;
			provided.Size             = new(0.47f, 0.1f);

			provided.Origin = Anchor.BottomCentre;
			provided.Anchor = Anchor.BottomCentre;
			flow.Add(provided);
		}

		protected override void Dispose(bool isDisposing)
		{
			NotificationsProvider.OnNotificationAdded -= addNotification;
			NotificationsProvider.OnNotificationRemoved -= removeNotification;

			base.Dispose(isDisposing);
		}
	}
}