using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Bindables;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Modding;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.Injection.Dependency;
using GameHost.Input.OpenTKBackend;
using GameHost.IO;
using NetFabric.Hyperlinq;
using OpenToolkit.Windowing.Common.Input;

namespace PataNext.Module.Presentation.Controls
{
	// todo: Temporary file, it should be replaced by a real abstract class that will be able to load other interfaces...
	public class LoadFileSystem : AppSystem
	{
		private IScheduler scheduler;
		private CModule    module;

		private Action fileDependencyResolver;

		private bool reloadFile;

		public Bindable<string> Xaml;

		public LoadFileSystem(WorldCollection collection) : base(collection)
		{
			Xaml = new Bindable<string>();

			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref module);

			reloadFile = true;
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			var storage = new StorageCollection {module.Storage.Value, module.DllStorage};
			fileDependencyResolver = () =>
			{
				var sw = new Stopwatch();
				sw.Start();

				storage.GetOrCreateDirectoryAsync("Interfaces").ContinueWith(t =>
				{
					// Add it as a dep
					var resolver = new DependencyResolver(scheduler, Context, "LoadFileSystem.GetFile");
					resolver.AddDependency(new FileDependency("MenuEntryControl.xaml", t.Result));
					resolver.OnComplete(async deps =>
					{
						foreach (var dep in deps)
						{
							if (!(dep is IFile file))
								continue;

							var result = Encoding.UTF8.GetString(await file.GetContentAsync());
							//Console.WriteLine(result);
							scheduler.Add(() => Xaml.Value = result);
						}
					});
				});
			};
		}

		public void DoReloadFile()
		{
			scheduler.Add(() => reloadFile = true);
		}

		protected override void OnUpdate()
		{
			if (reloadFile)
			{
				Console.WriteLine("reload file");
				reloadFile = false;
				fileDependencyResolver();
			}
		}

		// quite hacky here, once we will get a real input support we will not have the need to get our inputs
		// throught a custom restricted system...
		[RestrictToApplication(typeof(GameInputThreadingHost))]
		public class RestrictedHost : AppSystem
		{
			private OpenTkInputBackend inputBackend;
			private LoadFileSystem     loadFileSystem;

			public RestrictedHost(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref inputBackend);
				DependencyResolver.Add(() => ref loadFileSystem, new GetSystemFromTargetWorldStrategy(() =>
				{
					if (ThreadingHost.TryGetListener(out GameRenderThreadingHost host))
						return host.MappedWorldCollection.FirstOrDefault().Value;
					return null;
				}));
			}

			protected override void OnUpdate()
			{
				base.OnUpdate();
				if (inputBackend.IsKeyDown(Key.R))
				{
					loadFileSystem.DoReloadFile();
				}
			}
		}
	}
}