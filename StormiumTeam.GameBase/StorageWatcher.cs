using System;
using System.Collections.Generic;
using System.IO;
using GameHost.Core.IO;
using GameHost.IO;

namespace StormiumTeam.GameBase
{
	public class StorageWatcher : IDisposable
	{
		private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

		public event FileSystemEventHandler OnAnyUpdate;
		public string[]                     Filters;

		public StorageWatcher(params string[] filters)
		{
			Filters = filters;
		}
		
		public StorageWatcher(IStorage storage)
		{
			Add(storage);
		}

		public void Add(IStorage storage)
		{
			void addWatcher(FileSystemWatcher watcher)
			{
				if (Filters != null)
					foreach (var filter in Filters)
						watcher.Filters.Add(filter);

				watcher.NotifyFilter = NotifyFilters.LastWrite
				                       | NotifyFilters.FileName
				                       | NotifyFilters.CreationTime;

				watcher.Created += OnAnyUpdate;
				watcher.Changed += OnAnyUpdate;
				watcher.Deleted += OnAnyUpdate;

				watcher.EnableRaisingEvents = true;

				watchers.Add(watcher);
			}
			
			switch (storage)
			{
				case LocalStorage localStorage:
					addWatcher(new FileSystemWatcher(localStorage.CurrentPath ?? throw new InvalidOperationException("did not expect null")));
					break;
				case StorageCollection collection:
				{
					foreach (var child in collection)
					{
						Add(child);
					}

					break;
				}
				case ChildStorage childStorage:
				{
					if (childStorage.parent != null)
						Add(childStorage.parent);
					
					break;
				}
			}
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