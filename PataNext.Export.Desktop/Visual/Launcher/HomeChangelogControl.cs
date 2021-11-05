using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace PataNext.Export.Desktop.Visual
{
	public class HomeChangelogControl : Container
	{
		private readonly TextFlowContainer logs;

		public HomeChangelogControl()
		{
			Child = new BasicScrollContainer()
			{
				Size = new(1),
				RelativeSizeAxes = Axes.Both,
				
				Children = new Drawable[]
				{
					logs = new(text => { text.Font = new("ar_cena", 20f); })
					{
						Size                                           = new(1, 800),
						AutoSizeAxes                                   = Axes.Y,
						RelativeSizeAxes                               = Axes.X,

						AlwaysPresent = true
					},
				}
			};
		}

		protected override void UpdateAfterChildren()
		{
			base.UpdateAfterChildren();

			//Console.WriteLine((Child as BasicScrollContainer).Current);
		}

		public void Set(string currentVersion, Dictionary<string, string[]> changelogVersionMap)
		{
			logs.Clear();
			logs.AddText(" \n");
			
			foreach (var (version, lines) in changelogVersionMap)
			{
				logs.AddText("•  ", text => text.Font = text.Font.With(size: 32f));
				logs.AddText($"{version} ", text =>
				{
					text.Font   = new("mojipon", 32f);
					text.Colour = Colour4.FromHex("b81c31");
				});
				if (version == currentVersion)
					logs.AddText("(current)", text =>
					{
						text.Font   = new("ar_cena", 25f);
						text.Colour = Colour4.Gray;
					});

				logs.AddText("\n");
				foreach (var line in lines)
				{
					if (line.StartsWith("##")) // header
					{
						logs.AddText("\n", text => text.Font = new FontUsage("ar_cena", 5));
						logs.AddParagraph("  " + line[2..], text =>
						{
							text.Font         = new("ar_cena", 30f);
							text.Shadow       = true;
							text.ShadowColour = Colour4.Black;
							text.ShadowOffset = new Vector2(0.05f);
						});
						logs.AddText("\n", text => text.Font = new FontUsage("ar_cena", 5));
					}
					else
						logs.AddParagraph(line);
				}

				logs.AddText("\n\n\n\n");
			}
			
			logs.AddText("\n\n\n\n");
		}
	}
}