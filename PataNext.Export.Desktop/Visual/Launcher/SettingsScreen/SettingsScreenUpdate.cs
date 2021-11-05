using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK;
using PataNext.Export.Desktop.Visual.Configuration;
using PataNext.Export.Desktop.Visual.Overlays;

namespace PataNext.Export.Desktop.Visual.SettingsScreen
{
	public class SettingsScreenUpdate : SettingsScreenBase
	{
		[BackgroundDependencyLoader]
		private void load(LauncherConfigurationManager config)
		{
			var dropdown = new BasicDropdown<UpdateChannel>();
			dropdown.AddDropdownItem(UpdateChannel.Release);
			dropdown.AddDropdownItem(UpdateChannel.Beta);
			dropdown.Current.Value = config.GetBindable<UpdateChannel>(LauncherSetting.UpdateChannel).Value;
			dropdown.Width         = 200;
			
			dropdown.Current.BindValueChanged(ev =>
			{
				config.GetBindable<UpdateChannel>(LauncherSetting.UpdateChannel).Value = ev.NewValue;
			});

			AddSetting("Update Channel", dropdown, 30, "Strategy");
		}
	}
}