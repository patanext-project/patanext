using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using GameHost.Game;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;
using osuTK.Input;
using PataNext.Export.Desktop.Visual;
using PataNext.Export.Desktop.Visual.Overlays;
using PataNext.Export.Desktop.Visual.Screens;

namespace PataNext.Export.Desktop
{
	public class VisualGame : VisualGameBase
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
			IntPtr handle;
			handle = Window switch
			{
				SDL2DesktopWindow sdlWindow => sdlWindow.WindowHandle,
				OsuTKWindow tkWindow => tkWindow.WindowInfo.Handle
			};

			gameBootstrap.GameEntity.Set(new VisualHWND {Value = handle});
			dependencies.Cache(this);
			dependencies.Cache(gameBootstrap);
			dependencies.Cache(notifications);
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
				case SDL2DesktopWindow desktopGameWindow:
					desktopGameWindow.SetIconFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "game.ico"));
					desktopGameWindow.Title  = "PataNext";
					// desktopGameWindow.Size   = new Size(1024, 512); // how can I set the size of a sdl window?
					break;
				
				case OsuTKWindow desktopWindow:
					desktopWindow.Title = Name;
					desktopWindow.Size  = new Size(1024, 512);
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
			
			//LoadComponentAsync(new SquirrelUpdater(), Add);
			begin = DateTime.Now;
			
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
			Add(ScreenStack = new ScreenStack());
			
			ScreenStack.Push(new MainScreen());
		}

		private bool showIntegrated = true;

		protected override void Update()
		{
			base.Update();
			
			if (gameBootstrap?.GameEntity.World == null)
				return;
			
			IntPtr handle;
			handle = Window switch
			{
				SDL2DesktopWindow sdlWindow => sdlWindow.WindowHandle,
				OsuTKWindow tkWindow => tkWindow.WindowInfo.Handle
			};

			gameBootstrap.GameEntity.Set(new VisualHWND
			{
				Value = handle, 
				Size =
				{
					X = Window.ClientSize.Width,
					Y = Window.ClientSize.Height
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