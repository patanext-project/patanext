using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Core.Threading;
using GameHost.Inputs.Systems;
using GameHost.IO;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Worlds;
using Newtonsoft.Json;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Simulation.Client.Systems.Inputs
{
	public abstract class RegisterInputSystemBase<TInputSettings> : GameAppSystem
	{
		protected bool InputsAreCreated { get; private set; }

		protected InputDatabase InputDatabase;

		private GlobalWorld globalWorld;

		private IScheduler     scheduler;
		private GameHostModule module;

		protected TInputSettings InputSettings;

		private StorageWatcher storageWatcher;

		protected virtual string FileName => $"{typeof(TInputSettings).Name}.json";

		public RegisterInputSystemBase(WorldCollection collection) : base(collection)
		{
			AddDisposable(storageWatcher = new StorageWatcher(FileName));
			storageWatcher.OnAnyUpdate += onFileUpdate;

			DependencyResolver.Add(() => ref globalWorld);
			
			DependencyResolver.Add(() => ref InputDatabase);

			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref module);
		}

		private void onFileUpdate(object sender, FileSystemEventArgs e)
		{
			if ((e.ChangeType & (WatcherChangeTypes.Created | WatcherChangeTypes.Changed)) == 0)
				return;

			// It is possible to have multiple worlds in different thread with this system.
			// We need to be sure that those two systems does not read at the same the file (to not have it locked and throw an exception)
			// So we schedule the IO work on the main thread, and return the result and recreate input data on this world thread.
			globalWorld.Scheduler.Schedule(() =>
			{
				using var stream = new MemoryStream(File.ReadAllBytes(e.FullPath));
				using var reader = new JsonTextReader(new StreamReader(stream));

				var serializer = new JsonSerializer();
				var incomingInputSettings = serializer.Deserialize<TInputSettings>(reader);
				scheduler.Schedule(set =>
				{
					InputSettings = set;
					CreateInputs(set, true);
				}, incomingInputSettings, default);
			}, default);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			module.Storage.Subscribe((_, moduleStorage) =>
			{
				module.Storage.UnsubscribeCurrent();

				scheduler.Schedule(() =>
				{
					var storage = new StorageCollection {moduleStorage, module.DllStorage}.GetOrCreateDirectoryAsync("Inputs").Result;
					storageWatcher.Add(storage);

					using var stream = new MemoryStream(storage.GetFilesAsync(FileName).Result.First().GetContentAsync().Result);
					using var reader = new JsonTextReader(new StreamReader(stream));

					var serializer = new JsonSerializer();
					InputSettings = serializer.Deserialize<TInputSettings>(reader);
					CreateInputs(InputSettings, false);
					InputsAreCreated = true;
				}, default);
			}, true);
		}
		
		

		protected abstract void CreateInputs(in TInputSettings input, bool isUpdate);
	}
}