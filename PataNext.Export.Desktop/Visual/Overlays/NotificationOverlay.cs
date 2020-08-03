﻿using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
 using osu.Framework.Graphics.Effects;
 using osu.Framework.Threading;
 using osuTK;

 namespace PataNext.Export.Desktop.Visual.Overlays
{
	public class NotificationOverlay : FocusedOverlayContainer
	{
		/// <summary>
		/// Provide a source for the toolbar height.
		/// </summary>
		public Func<float> GetToolbarHeight;
		
		private FlowContainer<NotificationSection> sections;
		
		[BackgroundDependencyLoader]
		private void load()
		{
			Width            = 450;
			RelativeSizeAxes = Axes.Y;

			Margin = new MarginPadding {Top = 25, Right = 15};
			Scale   = Vector2.One * 0.95f;

			Children = new Drawable[]
			{
				new BasicScrollContainer
				{
					Masking          = true,
					RelativeSizeAxes = Axes.Both,
					EdgeEffect = new EdgeEffectParameters
					{
						Roundness = 10,
						Radius    = 5,
						Type = EdgeEffectType.Shadow,
						Colour = Colour4.Black,
					},
					Children = new[]
					{
						sections = new FillFlowContainer<NotificationSection>
						{
							Direction        = FillDirection.Vertical,
							AutoSizeAxes     = Axes.Y,
							RelativeSizeAxes = Axes.X,
							Children = new[]
							{
								new NotificationSection(@"Notifications", @"Clear All")
								{
									AcceptTypes = new[] {typeof(SimpleNotification)}
								},
								new NotificationSection(@"Running Tasks", @"Cancel All")
								{
									AcceptTypes = new[] {typeof(ProgressNotification)}
								}
							}
						}
					}
				}
			};
		}
		
		protected override void LoadComplete()
		{
			base.LoadComplete();
		}

		public readonly BindableInt UnreadCount = new BindableInt();
		
		private          void      notificationClosed() => updateCounts();
		private          int       runningDepth;
		private readonly Scheduler postScheduler = new Scheduler();
		
		protected override void Update()
		{
			base.Update();
			postScheduler.Update();
		}
		
		public void Post(Notification notification) => postScheduler.Add(() =>
		{
			++runningDepth;

			notification.Closed += notificationClosed;
			
			var ourType = notification.GetType();

			var section = sections.Children.FirstOrDefault(s => s.AcceptTypes.Any(accept => accept.IsAssignableFrom(ourType)));
			section?.Add(notification, notification.DisplayOnTop ? -runningDepth : runningDepth);

			if (notification.IsImportant)
				Show();

			updateCounts();
		});
		
		protected override void PopIn()
		{
			base.PopIn();

			this.MoveToX(0, 250, Easing.Out);
			this.FadeTo(1, 250, Easing.Out);
		}
		
		protected override void PopOut()
		{
			base.PopOut();

			markAllRead();
			
			this.MoveToX(325, 400, Easing.OutQuint);
			this.FadeTo(0, 400, Easing.OutQuint);
		}

		private void updateCounts()
		{
			UnreadCount.Value = sections.Select(c => c.UnreadCount).Sum();
		}

		private void markAllRead()
		{
			sections.Children.ForEach(s => s.MarkAllRead());

			updateCounts();
		}

		protected override void UpdateAfterChildren()
		{
			base.UpdateAfterChildren();

			Padding = new MarginPadding { Top = GetToolbarHeight?.Invoke() ?? 0 };
		}
	}
}