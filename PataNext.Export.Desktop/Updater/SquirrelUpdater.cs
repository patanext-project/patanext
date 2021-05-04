/*using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osuTK;
using osuTK.Graphics;
using PataNext.Export.Desktop.Visual.Overlays;
using PataNext.Game.Updater;
using Squirrel;

namespace PataNext.Export.Desktop.Updater
{
	public class SquirrelUpdater : GameUpdater
	{
		private UpdateManager updateManager;

		private static readonly Logger logger = Logger.GetLogger("updater");

		public override async Task CheckForUpdate() => await checkForUpdate(true);

		private UpdateProgressNotification notification;
		
		public Task PrepareUpdateAsync() => UpdateManager.RestartAppWhenExited();

		private async Task checkForUpdate(bool deltaPatching = true)
		{
			var rescheduleRecheck = true;
			try
			{
				updateManager ??= await getUpdateManager();
				if (!updateManager.IsInstalledApp)
					return;

				try
				{					
					var info = await updateManager.CheckForUpdate(!deltaPatching);
					if (info.ReleasesToApply.Count == 0)
					{
						logger.Add("No Updates found!");
						return;
					}

					if (notification == null)
					{
						notification = new UpdateProgressNotification(this) {State = ProgressNotificationState.Active};
						Schedule(() => Notifications.Post(notification));
					}

					notification.Progress = 0;
					notification.Text     = @"Downloading update...";

					logger.Add("Update Found!");
					await updateManager.DownloadReleases(info.ReleasesToApply, p => notification.Progress = p / 100f);

					notification.Progress = 0;
					notification.Text     = @"Installing update...";

					await updateManager.ApplyReleases(info, p => notification.Progress = p / 100f);
					notification.State = ProgressNotificationState.Completed;
					logger.Add("Finished");
				}
				catch (Exception ex)
				{
					if (deltaPatching)
					{
						logger.Add("Delta patching failed. Attempt to fully redownload.");

						await checkForUpdate(false);
						rescheduleRecheck = false;
					}
					else
					{
						notification.State = ProgressNotificationState.Cancelled;
						notification.Text  = "Update failed :(\nRe-Downloading the full setup is recommended.";
						Logger.Error(ex, "Update failed!");
					}
				}
			}
			catch (Exception ex)
			{
				// ignored
			}
			finally
			{
				if (rescheduleRecheck)
					Scheduler.AddDelayed(async () => await CheckForUpdate(), TimeSpan.FromMinutes(30).TotalMilliseconds);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);

			if (updateManager != null)
				updateManager.Dispose();
		}

		private async Task<UpdateManager> getUpdateManager()
		{
			return await UpdateManager.GitHubUpdateManager(@"https://github.com/guerro323/patanext", "PataNext");
		}
		
		private class UpdateProgressNotification : ProgressNotification
		{
			private readonly SquirrelUpdater updateManager;
			private          VisualGame         game;

			public UpdateProgressNotification(SquirrelUpdater updateManager)
			{
				this.updateManager = updateManager;
			}

			protected override Notification CreateCompletionNotification()
			{
				return new ProgressCompletionNotification
				{
					Text = @"Update ready to install. Click to restart!",
					Activated = () =>
					{
						updateManager.PrepareUpdateAsync()
						             .ContinueWith(_ => updateManager.Schedule(() => game.GracefullyExit()));
						return true;
					}
				};
			}

			[BackgroundDependencyLoader]
			private void load(VisualGame game)
			{
				this.game = game;

				IconContent.AddRange(new Drawable[]
				{
					new Box
					{
						RelativeSizeAxes = Axes.Both,
						Colour           = ColourInfo.GradientVertical(Colour4.DarkGoldenrod, Colour4.Yellow)
					},
					new SpriteIcon
					{
						Anchor = Anchor.Centre,
						Origin = Anchor.Centre,
						Icon   = FontAwesome.Solid.Upload,
						Colour = Color4.White,
						Size   = new Vector2(20),
					}
				});
			}
		}
	}
}*/