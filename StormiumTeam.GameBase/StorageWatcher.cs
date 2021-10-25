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

			void addWatcherOP(FileSystemWatcher watcher)
			{
#if NETSTANDARD
				if (Filters == null || Filters.Length == 0)
				{
					addWatcher(watcher);
				}
				else if (Filters.Length == 1)
				{
					watcher.Filter = Filters[0];
					addWatcher(watcher);
				}
				else if (Filters.Length > 1)
				{
					foreach (var filter in Filters.AsSpan(1))
					{
						watcher = new(watcher.Path, filter);
						addWatcher(watcher);
					}
				}
#else
				if (Filters != null)
					foreach (var filter in Filters)
						watcher.Filters.Add(filter);

				addWatcher(watcher);
#endif
			}

			switch (storage)
			{
				case LocalStorage localStorage:
					addWatcherOP(new FileSystemWatcher(localStorage.CurrentPath ?? throw new InvalidOperationException("did not expect null")));
					return true;
				case StorageCollection collection:
					return collection.Aggregate(false, (current, child) => current | Add(child));
				case ChildStorage childStorage:
					return childStorage.parent != null && Add(childStorage.parent);
				case IStorageProvideWatcher provider:
				{
					addWatcherOP(provider.CreateWatcher());
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