using System;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using PataNext.Game.Updater;
using Squirrel;

namespace PataNext.Export.Desktop.Updater
{
	public class SquirrelUpdater : GameUpdater
	{
		private UpdateManager updateManager;

		private static readonly Logger logger = Logger.GetLogger("updater");

		public override async Task CheckForUpdate() => await checkForUpdate(true);

		private async Task checkForUpdate(bool deltaPatching = true)
		{
			var rescheduleRecheck = true;
			try
			{
				updateManager ??= await getUpdateManager();
				/*if (updateManager.IsInstalledApp)
					Console.WriteLine("This is an installed application.");
				else
				{
					Console.WriteLine("This is not an installed application.");
					return;
				}*/

				try
				{
					var info = await updateManager.CheckForUpdate(!deltaPatching);
					if (info.ReleasesToApply.Count == 0)
					{
						logger.Add("No Updates found!");
						return;
					}

					logger.Add("Update Found!");
					await updateManager.DownloadReleases(info.ReleasesToApply, p => { Console.WriteLine($"Download Progress: {p}%"); });
					await updateManager.ApplyReleases(info, p => { Console.WriteLine($"Applying Update: {p}%"); });
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
				}
			}
			catch (Exception ex)
			{
				// ignored
			}
			finally
			{
				if (rescheduleRecheck)
					Scheduler.AddDelayed(async () => await CheckForUpdate(), TimeSpan.FromMinutes(2).TotalMilliseconds);
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
			return new UpdateManager(@"https://github.com/guerro/patanext", "PataNext");
		}
	}
}