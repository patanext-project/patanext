using System;
using System.Reflection;
using GameHost.Game;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;
using osuTK.Input;
using PataNext.Export.Desktop.Updater;
using PataNext.Export.Desktop.Visual;
using PataNext.Export.Desktop.Visual.Overlays;
using PataNext.Export.Desktop.Visual.Screens;

namespace PataNext.Export.Desktop
{
	public class VisualGame : osu.Framework.Game
	{
		private readonly NotificationOverlay notifications = new NotificationOverlay();
		
		private GameBootstrap gameBootstrap;
		private Box           box;

		public VisualGame(GameBootstrap gameBootstrap)
		{
			this.gameBootstrap = gameBootstrap;
		}
		
		private DependencyContainer dependencies;
		
		protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
			dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

		[BackgroundDependencyLoader]
		private void load()
		{
			gameBootstrap.GameEntity.Set(new VisualHWND {Value = Window.WindowInfo.Handle});
			dependencies.Cache(this);
			dependencies.Cache(gameBootstrap);
			dependencies.Cache(notifications);

			Child = new DrawSizePreservingFillContainer
			{
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre,
				Children = new Drawable[]
				{
					new Box
					{
						Anchor               = Anchor.Centre,
						Origin               = Anchor.Centre,
						RelativePositionAxes = Axes.Both,
						RelativeSizeAxes     = Axes.Both,
						Colour               = Colour4.Black,
						Size                 = new Vector2(1),
					},
					new GameHostApplicationRunner(),
					//new OsuInputBackend(),
				}
			};
		}

		private DateTime begin;
		private ScreenStack ScreenStack;

		public VisualGame()
		{
			Name = "PataNext";
		}
		
		public override void SetHost(osu.Framework.Platform.GameHost host)
		{
			base.SetHost(host);

			foreach (var r in Assembly.GetExecutingAssembly().GetManifestResourceNames())
			{
				Console.WriteLine("------------ " + r);
			}
			
			switch (host.Window)
			{
				// Legacy osuTK DesktopGameWindow
				case DesktopGameWindow desktopGameWindow:
					desktopGameWindow.SetIconFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "game.ico"));
					desktopGameWindow.Title = "PataNext";
					desktopGameWindow.Width = 1024;
					desktopGameWindow.Height = 512;
					break;

				// SDL2 DesktopWindow
				case DesktopWindow desktopWindow:
					desktopWindow.Title             =  Name;
					break;
			}
		}

		public void GracefullyExit()
		{
			if (!OnExiting())
				Exit();
			else
				Scheduler.AddDelayed(GracefullyExit, 2000);
		}
		
		protected override void LoadComplete()
		{
			base.LoadComplete();
			
			LoadComponentAsync(new SquirrelUpdater(), Add);
			begin = DateTime.Now;
			
			Add(ScreenStack = new ScreenStack());
			
			ScreenStack.Push(new MainScreen());
		}

		private bool showIntegrated = true;

		protected override void Update()
		{
			base.Update();
			
			gameBootstrap.GameEntity.Set(new VisualHWND
			{
				Value = Window.WindowInfo.Handle, Size =
				{
					X = Window.Width,
					Y = Window.Height
				},
				ShowIntegratedWindows = showIntegrated && begin.AddSeconds(2) < DateTime.Now
			});
		}

		protected override bool OnKeyDown(KeyDownEvent e)
		{
			if (e.Key == Key.G && e.ControlPressed)
			{
				showIntegrated = !showIntegrated;
			}

			return base.OnKeyDown(e);
		}

		protected override bool OnExiting()
		{
			gameBootstrap.Dispose();
			return false;
		}
	}
}