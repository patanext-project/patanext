using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameHost.Core.IO;
using GameHost.IO;

namespace StormiumTeam.GameBase
{
	public interface IStorageProvideWatcher
	{
		FileSystemWatcher CreateWatcher();
	}
	
	public class StorageWatcher : IDisposable
	{
		private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

		public event FileSystemEventHandler OnAnyUpdate;
		public string[]                     Filters;

		public StorageWatcher(params string[] filters)
		{
			Filters = filters;
		}

		public bool Add(IStorage storage)
		{
			void addWatcher(FileSystemWatcher watcher)
			{
				if (Filters != null)
					foreach (var filter in Filters)
						watcher.Filters.Add(filter);

				watcher.NotifyFilter = NotifyFilters.LastWrite
				                       | NotifyFilters.FileName
				                       | NotifyFilters.CreationTime;

				watcher.Created += (sender, args) =>
				{
					OnAnyUpdate?.Invoke(sender, args);
				};
				watcher.Changed += (sender, args) =>
				{
					OnAnyUpdate?.Invoke(sender, args);
				};
				watcher.Deleted += (sender, args) =>
				{
					OnAnyUpdate?.Invoke(sender, args);
				};

				watcher.EnableRaisingEvents = true;

				watchers.Add(watcher);
			}
			
			switch (storage)
			{
				case LocalStorage localStorage:
					addWatcher(new FileSystemWatcher(localStorage.CurrentPath ?? throw new InvalidOperationException("did not expect null")));
					return true;
				case StorageCollection collection:
					return collection.Aggregate(false, (current, child) => current | Add(child));
				case ChildStorage childStorage:
					return childStorage.parent != null && Add(childStorage.parent);
				case IStorageProvideWatcher provider:
				{
					addWatcher(provider.CreateWatcher());
					return true;
				}
			}

			return false;
		}

		public void Dispose()
		{
			foreach (var watcher in watchers)
			{
				watcher.EnableRaisingEvents = false;
				watcher.Dispose();
			}

			watchers.Clear();
		}
	}
}