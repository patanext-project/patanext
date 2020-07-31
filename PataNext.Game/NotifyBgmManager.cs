using System;
using System.Collections.Generic;
using System.IO;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.IO;
using GameHost.Simulation.Application;
using System.Linq;
using PataNext.Client.Systems;
using PataNext.Game.BGM;

namespace PataNext.Game
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class NotifyBgmManager : AppSystem
	{
		private BgmContainerStorage bgmStorage;

		public NotifyBgmManager(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref bgmStorage);
		}

		private List<FileSystemWatcher> watchers;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			watchers = new List<FileSystemWatcher>();
			switch (bgmStorage.parent)
			{
				case LocalStorage localStorage:
					watchers.Add(new FileSystemWatcher(localStorage.CurrentPath));
					break;
				case StorageCollection collection:
				{
					foreach (var storage in collection)
						if (storage is LocalStorage local)
							watchers.Add(new FileSystemWatcher(local.CurrentPath));
					break;
				}
			}

			foreach (var watcher in watchers)
			{
				watcher.Filters.Add("*.zip");
				watcher.Filters.Add("*.json");
				watcher.NotifyFilter = NotifyFilters.LastWrite
				                       | NotifyFilters.FileName
				                       | NotifyFilters.CreationTime;

				watcher.Created += onChange;
				watcher.Changed += onChange;
				watcher.Deleted += onChange;
				watcher.Renamed += onChange;

				watcher.EnableRaisingEvents = true;
			}
		}

		private void onChange(object source, FileSystemEventArgs e)
		{
			Console.WriteLine($"File Update: {e.FullPath} {e.ChangeType}");
			World.Mgr.CreateEntity()
			     .Set<RefreshBgmList>();
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var watcher in watchers)
			{
				watcher.EnableRaisingEvents = false;
				watcher.Dispose();
			}

			watchers.Clear();
		}
	}
}