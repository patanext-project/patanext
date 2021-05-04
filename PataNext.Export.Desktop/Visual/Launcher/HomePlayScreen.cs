using System;
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
				if (ProgressPlayBar.RequireRestart.Value && Dependencies.TryGet(out IUpdatePatchDownload patch))
					Scheduler.AddOnce(() => patch.UpdateAndRestart());
				else if (!ProgressPlayBar.RequireRestart.Value)
				{
					Console.WriteLine("launch game");
				}
			};
		}

		public void PushPopup(string title, string description, Action action, Texture texture = null)
		{
			flow.Add(new NotificationPopup
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
				Icon  = texture
			});
		}

		[Resolved]
		private IGlobalNotifications notifications { get; set; }

		[BackgroundDependencyLoader]
		private void load(IUpdatePatchDownload updatePatch, ICurrentVersion currentVersion)
		{
			updatePatch.IsUpdating.BindValueChanged(ev => ProgressPlayBar.IsUpdating.Value   = ev.NewValue, true);
			updatePatch.Progress.BindValueChanged(ev => ProgressPlayBar.UpdateProgress.Value = ev.NewValue, true);
			
			updatePatch.RequireRestart.BindValueChanged(ev =>
			{
				ProgressPlayBar.RequireRestart.Value = ev.NewValue;
			}, true);
			
			currentVersion.Current.BindValueChanged(ev => ProgressPlayBar.CurrentVersion.Value = ev.NewValue, true);
			
			notifications.OnNotificationAdded += addNotification;
		}

		private void addNotification(NotificationBase n)
		{
			if (!(n is NotificationPopup popup))
				return;

			popup.FillAspectRatio = 906f / 89f;
			popup.FillMode        = FillMode.Fit;
				
			popup.RelativeSizeAxes = Axes.Both;
			popup.Size             = new(0.47f, 0.1f);

			popup.Origin = Anchor.BottomCentre;
			popup.Anchor = Anchor.BottomCentre;
			flow.Add(popup);
		}

		protected override void Dispose(bool isDisposing)
		{
			notifications.OnNotificationAdded -= addNotification;
			
			base.Dispose(isDisposing);
		}
	}
}