using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace PataNext.Export.Desktop.Visual.Configuration
{
	public class LauncherConfigurationManager : IniConfigManager<LauncherSetting>
	{
		protected override void InitialiseDefaults()
		{
			SetDefault(LauncherSetting.UpdateChannel, UpdateChannel.Beta);
			
			base.InitialiseDefaults();
			
			
		}

		public LauncherConfigurationManager(Storage storage, IDictionary<LauncherSetting, object> defaultOverrides = null) : base(storage, defaultOverrides)
		{
		}
	}

	public enum LauncherSetting
	{
		UpdateChannel
	}
}