using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using GameHost.Game;
using GameHost.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osuTK;
using PataNext.Export.Desktop.Visual;
using PataNext.Export.Desktop.Visual.Configuration;
using PataNext.Export.Desktop.Visual.Dependencies;
using PataNext.Export.Desktop.Visual.Screens;
using PataNext.Export.Desktop.Visual.Screens.Section;
using SharpInputSystem;
using SharpInputSystem.DirectX;

namespace PataNext.Export.Desktop.Tests.Visual
{
	public class LauncherScene : TestScene
	{
		public Bindable<int> bindable = new(2);

		private ClientSectionScreen screen;

		private DependencyContainer dependencies;

		protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
		{
			return dependencies = new(base.CreateChildDependencies(parent));
		}
		

		[Resolved]
		private TextureStore textures { get; set; }

		[BackgroundDependencyLoader]
		private void load(Storage storage)
		{
			dependencies.CacheAs<ICurrentVersion>(new CurrentVersion()
			{
				Current = {Value = "2021.04.15"}
			});
			dependencies.CacheAs<IPatchProvider>(new PatchProvider(Scheduler)
			{

			}); 
			dependencies.CacheAs<INotificationsProvider>(new NotificationsProvider());
			dependencies.CacheAs<IChangelogProvider>(new WebChangelogProvider(new("https://raw.githubusercontent.com/guerro323/patanext/master/CHANGELOG.md"), Scheduler));
			dependencies.Cache(new LauncherConfigurationManager(storage));

			var gameBootstrap = new GameBootstrap(); // we don't need to have a full bootstrap here, we just need some dependencies data
			gameBootstrap.GameEntity.Set(new GameName("PataNext"));
			gameBootstrap.GameEntity.Set(new GameUserStorage(new LocalStorage(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/PataNext")));
			
			var inputManager = InputManager.CreateInputSystem(typeof(DirectXInputManagerFactory), new ParameterList
			{
				new("WINDOW", Process.GetCurrentProcess().MainWindowHandle)
			});
			dependencies.CacheAs(inputManager);

			gameBootstrap.Setup();
			
			dependencies.Cache(gameBootstrap);

			Add(new LauncherMainScene()
			{
				/*Strategy        = DrawSizePreservationStrategy.Minimum,
				FillAspectRatio = 16 / 9f,

				FillMode = FillMode.Fit,*/
			});

			/*n.Clicked += () =>
			{
				Logger.Log("launch update!", LoggingTarget.Information, LogLevel.Important);
			};*/

			AddToggleStep("IsUpdate", b => Dependencies.Get<IPatchProvider>().IsUpdating.Value                  = b);
			AddSliderStep("UpdateProgress", 0, 1, 0f, f => Dependencies.Get<IPatchProvider>().Progress.Value = f);
			AddStep("Add Update", () =>
			{
				var notifications = Dependencies.Get<INotificationsProvider>();

				var updateNotification = new Notification
				{
					Title = "Information",
					Text  = "Update Available!",
					Icon  = textures.Get("popup_update")
				};

				updateNotification.Action = () =>
				{
					var updatePatch = Dependencies.Get<IPatchProvider>();
					updatePatch.StartDownload();
					
					notifications.Remove(updateNotification);
				};

				notifications.Push(updateNotification);
			});
		}
	}

	class CurrentVersion : ICurrentVersion
	{
		public Bindable<string> Current { get; } = new();
	}

	class PatchProvider : IPatchProvider
	{
		public Bindable<string> Version        { get; } = new();
		public BindableBool     IsUpdating     { get; } = new();
		public BindableBool     RequireRestart { get; } = new();
		public BindableFloat    Progress       { get; } = new();

		private Scheduler scheduler;

		public PatchProvider(Scheduler scheduler)
		{
			this.scheduler = scheduler;
		}

		public void StartDownload()
		{
			IsUpdating.Value = true;
			Progress.Value   = 0f;
			scheduler.AddDelayed(() => { Progress.Value = 0.25f; }, 1000);
			scheduler.AddDelayed(() => { Progress.Value = 0.5f; }, 1500);
			scheduler.AddDelayed(() => { Progress.Value = 0.75f; }, 2000);
			scheduler.AddDelayed(() =>
			{
				Progress.Value       = 1f; 
				IsUpdating.Value     = false;
				RequireRestart.Value = true;
			}, 2500);
		}

		public void UpdateAndRestart()
		{
			Console.WriteLine("update and restart!");
		}
	}

	class NotificationsProvider : INotificationsProvider
	{
		private List<Notification> notifications = new();

		public IReadOnlyList<Notification> GetAll() => notifications;

		public void Push(Notification notification)
		{
			notifications.Add(notification);
			OnNotificationAdded?.Invoke(notification);
		}

		public void ClearAll()
		{
			foreach (var notification in notifications)
				OnNotificationRemoved?.Invoke(notification);
			notifications.Clear();
		}

		public void Clear(Type type)
		{
			foreach (var notification in notifications.Where(notification => notification.GetType().IsSubclassOf(type)))
				OnNotificationRemoved?.Invoke(notification);
			notifications.RemoveAll(n => n.GetType().IsSubclassOf(type));
		}

		public void Remove(Notification notification)
		{
			if (notifications.Remove(notification))
				OnNotificationRemoved?.Invoke(notification);
		}

		public event Action<Notification> OnNotificationAdded;
		public event Action<Notification> OnNotificationRemoved;
	}
}