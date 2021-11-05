using System;
using System.Collections.Generic;
using System.Net;
using osu.Framework.Bindables;
using osu.Framework.Threading;

namespace PataNext.Export.Desktop.Visual.Dependencies
{
	public interface IChangelogProvider
	{
		public Bindable<Dictionary<string, string[]>> Current { get; }
	}

	public class WebChangelogProvider : IChangelogProvider
	{
		public Bindable<Dictionary<string, string[]>?> Current { get; } = new();

		private readonly Scheduler scheduler;
		private readonly Uri       uri;

		public WebChangelogProvider(Uri uri, Scheduler scheduler)
		{
			this.scheduler = scheduler;
			this.uri       = uri;

			fetch();
		}

		private void fetch()
		{
			using var client = new WebClient();
			client.DownloadStringTaskAsync(uri)
			      .ContinueWith(str =>
			      {
				      parse(str.Result);

				      scheduler.AddDelayed(fetch, 30_000);
			      });
		}

		private void parse(string raw)
		{
			var map       = new Dictionary<string, string[]>();
			var inVersion = false;

			string version = "";

			var list = new List<string>();
			foreach (var txt in raw.Split('\n'))
			{
				if (txt.Length > 2 && txt[0] == '#' && txt[1] == ' ')
				{
					if (inVersion)
					{
						map[version] = list.ToArray();
						list.Clear();
					}
					
					inVersion = true;
					version   = txt[2..];
					continue;
				}

				if (!inVersion)
					continue;
				
				list.Add(txt);
			}

			if (list.Count > 0)
				map[version] = list.ToArray();

			scheduler.AddOnce(() => Current.Value = map);
		}
	}
}