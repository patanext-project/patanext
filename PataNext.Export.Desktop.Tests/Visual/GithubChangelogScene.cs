using System;
using System.Collections.Generic;
using System.Net;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osuTK;
using PataNext.Export.Desktop.Visual;
using PataNext.Export.Desktop.Visual.Dependencies;

namespace PataNext.Export.Desktop.Tests.Visual
{
	public class GithubChangelogScene : TestScene
	{
		[BackgroundDependencyLoader]
		private void load()
		{
			var changelog = new HomeChangelogControl {Size = Vector2.One, RelativeSizeAxes = Axes.Both};
			Child = changelog;

			var provider  = new WebChangelogProvider(new("https://raw.githubusercontent.com/guerro323/patanext/master/CHANGELOG.md"), Scheduler);
			provider.Current.BindValueChanged(ev =>
			{
				if (ev.NewValue == null)
					return;
				
				changelog.Set("test", ev.NewValue);
			});
		}
	}
}