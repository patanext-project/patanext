using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core;
using GameHost.Core.Applications;
using GameHost.Core.Bindables;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Threading;
using GameHost.IO;
using GameHost.UI.Noesis;
using OpenToolkit.Windowing.Common;
using PataponGameHost.Applications.MainThread;
using PataponGameHost.Storage;

namespace PataNext.Module.Presentation.Controls
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	[UpdateAfter(typeof(NoesisInitializationSystem))]
	public class MenuEntryControlSystem : AppSystem
	{
		private INativeWindow    window;
		private IScheduler       scheduler;
		private MainThreadClient client;
		private LoadFileSystem loadFileSystem;

		public MenuEntryControlSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref window);
			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref client);
			DependencyResolver.Add(() => ref loadFileSystem);
		}

		private MenuEntryControl.BgmEntry[] files = new MenuEntryControl.BgmEntry[0];

		private IStorage         storage;
		private Entity entityView;

		private void onBgmFileChange()
		{
			var clientWorld = client.Listener.WorldCollection;
			var arrayOfBgm  = clientWorld.Mgr.Get<BgmFile>().ToArray();

			void OnSetFiles()
			{
				files = (from file in arrayOfBgm
				         orderby file.Name
				         select new MenuEntryControl.BgmEntry {Content = file.Description}).ToArray();
			}

			scheduler.Add(OnSetFiles);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			Console.WriteLine("dependencies resolved");
			using (client.SynchronizeThread())
			{
				AddDisposable(client.Listener.WorldCollection.Mgr.SubscribeComponentChanged((in Entity _0, in BgmFile _1, in BgmFile _2) => onBgmFileChange()));
				AddDisposable(client.Listener.WorldCollection.Mgr.SubscribeComponentAdded((in   Entity _0, in BgmFile _1) => onBgmFileChange()));

				client.Listener.WorldCollection.Mgr.CreateEntity().Set<RefreshBgmList>();
			}
			
			loadFileSystem.Xaml.Subscribe(OnXamlFound, true);
		}

		private void OnXamlFound(string previous, string next)
		{
			if (next == null)
				return;
			
			void addXaml()
			{
				var view = new NoesisOpenTkRenderer(window);
				view.ParseXaml(next);

				entityView = entityView.IsAlive ? entityView : World.Mgr.CreateEntity();
				if (entityView.Has<NoesisOpenTkRenderer>())
				{
					var oldRenderer = entityView.Get<NoesisOpenTkRenderer>();
					oldRenderer.Dispose();
				}
				
				entityView.Set(view);
				entityView.Set((MenuEntryControl) view.View.Content);
			}

			scheduler.Add(addXaml);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			foreach (ref var control in World.Mgr.Get<MenuEntryControl>())
			{
				var view = control.DataContext as MenuEntryControl.ViewModel;
				if (view == null)
					continue;
				view.BgmEntries = files;
			}
		}
	}
}