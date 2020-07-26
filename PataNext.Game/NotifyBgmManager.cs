using System;
using System.Collections.Generic;
using System.IO;
using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
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

		private FileSystemWatcher watcher;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			watcher = new FileSystemWatcher(bgmStorage.CurrentPath);
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

		private void onChange(object source, FileSystemEventArgs e)
		{
			Console.WriteLine($"File Update: {e.FullPath} {e.ChangeType}");
			World.Mgr.CreateEntity()
			     .Set<RefreshBgmList>();
		}

		public override void Dispose()
		{
			base.Dispose();

			watcher.EnableRaisingEvents = false;
			watcher.Dispose();
		}
	}
}