using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using GameHost.Core.Ecs;
using GameHost.Game;
using GameHost.Inputs.Systems;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;
using osuTK.Input;
using PataNext.Export.Desktop.Providers;
using PataNext.Export.Desktop.Updater;
using PataNext.Export.Desktop.Visual;
using PataNext.Export.Desktop.Visual.Configuration;
using PataNext.Export.Desktop.Visual.Dependencies;
using PataNext.Export.Desktop.Visual.Overlays;
using PataNext.Export.Desktop.Visual.Screens;
using SharpInputSystem;
using SharpInputSystem.DirectX;
using Keyboard = SharpInputSystem.Keyboard;

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

		private Keyboard kb;

		[BackgroundDependencyLoader]
		private void load(FrameworkConfigManager configManager, Storage storage)
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

			var notificationsProvider = new NotificationsProvider();
			var accountProvider       = new StandardAccountProvider(notificationsProvider);
			gameBootstrap.Global.Context.BindExisting<IAccountProvider>(accountProvider);

			var version = Assembly.GetAssembly(typeof(VisualGame)).GetName().Version.ToString();
			if (version.EndsWith(".0"))
				version = version[..^2];
			
			dependencies.CacheAs<ICurrentVersion>(new CurrentVersion()
			{
				Current = {Value =version}
			});
			dependencies.CacheAs<IPatchProvider>(new PatchProvider(Scheduler, this)
			{

			});
			dependencies.CacheAs<INotificationsProvider>(notificationsProvider);
			dependencies.CacheAs<IChangelogProvider>(new WebChangelogProvider(new("https://raw.githubusercontent.com/guerro323/patanext/master/CHANGELOG.md"), Scheduler));
			dependencies.Cache(new LauncherConfigurationManager(storage));
			dependencies.CacheAs<IAccountProvider>(accountProvider);
			
			var inputManager = InputManager.CreateInputSystem(typeof(DirectXInputManagerFactory), new ParameterList
			{
				new("WINDOW", Process.GetCurrentProcess().MainWindowHandle)
			});
			dependencies.CacheAs(inputManager);
			
			(kb = inputManager.CreateInputObject<Keyboard>(true, "")).EventListener = new SharpDxInputSystem.KeyboardListenerSimple();

			configManager.GetBindable<FrameSync>(FrameworkSetting.FrameSync).Value = FrameSync.VSync;
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
			
			LoadComponentAsync(new SquirrelUpdater(), Add);
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
			
			ScreenStack.Push(new LauncherMainScene());
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

			if (gameBootstrap.GameEntity.TryGet(out VisualHWND prev))
			{
				if (prev.RequireSwap)
				{
					//(Window as SDL2DesktopWindow).Visible = false;
					(Window as SDL2DesktopWindow).Visible = true;
				}
			}

			gameBootstrap.GameEntity.Set(new VisualHWND
			{
				Value = Process.GetCurrentProcess().MainWindowHandle, 
				Size =
				{
					X = Window.ClientSize.Width,
					Y = Window.ClientSize.Height
				},
				ShowIntegratedWindows = showIntegrated && begin.AddSeconds(2) < DateTime.Now
			});

			kb.Capture();

			var listener = kb.EventListener as SharpDxInputSystem.KeyboardListenerSimple;
			if (listener.ControlMap[KeyCode.Key_G].IsPressed && kb.IsShiftState(Keyboard.ShiftState.Ctrl))
			{
				showIntegrated = !showIntegrated;
			}

			foreach (var c in listener.ControlMap)
				c.Value.IsPressed = false;
		}

		protected override bool OnKeyDown(KeyDownEvent e)
		{
			return base.OnKeyDown(e);
		}

		protected override bool OnExiting()
		{
			gameBootstrap.Dispose();
			return false;
		}
	}
}