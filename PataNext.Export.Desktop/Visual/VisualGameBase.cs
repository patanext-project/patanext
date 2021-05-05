using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Threading;
using PataNext.Export.Desktop.Visual.Configuration;
using PataNext.Export.Desktop.Visual.Dependencies;
using SharpInputSystem;
using SharpInputSystem.DirectX;
using Squirrel;

namespace PataNext.Export.Desktop.Visual
{
	public class VisualGameBase : osu.Framework.Game
	{
		protected DependencyContainer dependencies;

		protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

		[BackgroundDependencyLoader]
		private void load(Storage storage)
		{
			Resources.AddStore(new DllResourceStore(typeof(VisualGameBase).Assembly));
			Resources.AddStore(new DllResourceStore(typeof(PataNext.Game.Client.Resources.Module).Assembly));

			AddFont(Resources, @"Fonts/ar_cena");
			AddFont(Resources, @"Fonts/mojipon");
			AddFont(Resources, @"Fonts/roboto");
		}

		protected class CurrentVersion : ICurrentVersion
		{
			public Bindable<string> Current { get; } = new();
		}

		protected class PatchProvider : IPatchProvider
		{
			public Bindable<string> Version        { get; } = new();
			public BindableBool     IsUpdating     { get; } = new();
			public BindableBool     RequireRestart { get; } = new();
			public BindableFloat    Progress       { get; } = new();

			private Scheduler  scheduler;
			private VisualGame gameHost;

			public PatchProvider(Scheduler scheduler, VisualGame gameHost)
			{
				this.scheduler = scheduler;
				this.gameHost  = gameHost;
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
				UpdateManager.RestartAppWhenExited()
				             .ContinueWith(_ => scheduler.Add(() => gameHost.GracefullyExit()));
			}
		}

		protected class NotificationsProvider : INotificationsProvider
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
				foreach (var notification in notifications.Where(notification => notification.GetType().IsAssignableTo(type)))
					OnNotificationRemoved?.Invoke(notification);
				notifications.RemoveAll(n => n.GetType().IsAssignableTo(type));
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
}