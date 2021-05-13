using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osuTK;
using osuTK.Graphics;
using PataNext.Export.Desktop.Visual;
using PataNext.Export.Desktop.Visual.Configuration;
using PataNext.Game.Updater;
using Squirrel;

namespace PataNext.Export.Desktop.Updater
{
	public class UpdateNotification : Notification {}
	public class InstallingNotification : Notification {}
	
	public class SquirrelUpdater : GameUpdater
	{
		private UpdateManager updateManager;

		private static readonly Logger logger = Logger.GetLogger("updater");

		public override async Task CheckForUpdate() => await checkForUpdate(true);
		
		public Task PrepareUpdateAsync() => UpdateManager.RestartAppWhenExited();

		private async Task startUpdate(UpdateInfo info)
		{
			try
			{
				Schedule(() => Notifications.Clear(typeof(InstallingNotification)));
				
				Schedule(() => Patch.Progress.Value  = 0);

				logger.Add("Update Found!");
				await updateManager.DownloadReleases(info.ReleasesToApply, p => Schedule(() => Patch.Progress.Value = p / 100f));

				Schedule(() => Patch.Progress.Value = 0);

				Schedule(() => Notifications.Push(new InstallingNotification()
				{
					Title = "Installing Update",
					Text  = "Update is being installed, don't restart the game..."
				}));
				

				await updateManager.ApplyReleases(info, p => Schedule(() => Patch.Progress.Value = p / 100f));
				
				Schedule(() => Notifications.Clear(typeof(InstallingNotification)));
				Schedule(() => Patch.IsUpdating.Value = false);
				Schedule(() => Patch.RequireRestart.Value = true);
				logger.Add("Finished");
			}
			catch (Exception ex)
			{
				if (info.FutureReleaseEntry.IsDelta)
				{
					await checkForUpdate(false);
					return;
				}

				Schedule(() =>
				{
					Notifications.Push(new()
					{
						Title = "Error",
						Text = "Update failed, reinstalling the game may help to fix this issue."
					});
				});
				Logger.Error(ex, "Update failed!");
			}
		}
		
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

					// Only show the notification to update if deltaPatching is on.
					// If it isn't, this mean the user wanted to update but failed due to incorrect delta.
					// Then we don't need to ask the user again to re-update (the user will think it didn't updated correctly) and retry updating now
					if (deltaPatching)
					{
						Schedule(() =>
						{
							Notifications.Clear(typeof(UpdateNotification));
							Notifications.Push(new UpdateNotification()
							{
								Title = "Information",
								Text  = $"Update to '{info.FutureReleaseEntry.Version}' is available!",
								Action = () =>
								{
									Patch.Version.Value    = info.FutureReleaseEntry.Version.ToString();
									Patch.IsUpdating.Value = true;

									Notifications.Clear(typeof(UpdateNotification));

									Task.Run(async () => await startUpdate(info));
								}
							});
						});
					}
					else
					{
						await startUpdate(info);
					}
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
			var isPreRelease = Configuration.Get<UpdateChannel>(LauncherSetting.UpdateChannel) == UpdateChannel.Beta;
			return await UpdateManager.GitHubUpdateManager(@"https://github.com/guerro323/patanext", "PataNext", prerelease: isPreRelease);
		}
		
		/*private class UpdateProgressNotification : ProgressNotification
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
		}*/
	}
}