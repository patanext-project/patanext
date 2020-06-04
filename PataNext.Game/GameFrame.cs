using System;
using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Core.Threading;
using GameHost.Injection;
using ImTools;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;

namespace PataponGameHost
{	
	public class GameFrame : GameWindow
	{
		private Context context;

		private List<IDisposable> disposables;
		
		private GameRenderThreadingHost renderHost;
		private GameAudioThreadingHost audioHost;

		public GameFrame(Context context, List<IDisposable> disposableList) : base(new GameWindowSettings
		{
			IsMultiThreaded = false,
			RenderFrequency = 500,
			UpdateFrequency = 500,
		}, new Func<NativeWindowSettings>(() =>
		{
			var s = new NativeWindowSettings
			{
				Title        = "PataNext.Game",
				Size         = new OpenToolkit.Mathematics.Vector2i {X = 1280, Y = 720},
				API          = ContextAPI.OpenGL,
				IsFullscreen = true,
				//WindowBorder = WindowBorder.Hidden
			};
			//GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
			GLFW.WindowHint(WindowHintInt.Samples, 2);
			
			return s;
		})())
		{
			this.context = context;
			this.disposables = disposableList;

			VSync = VSyncMode.Off;
		}

		protected override unsafe void OnLoad()
		{
			base.OnLoad();

			GL.LoadBindings(new GLFWBindingsContext());
			GLFW.MakeContextCurrent(null);
			
			renderHost = new GameRenderThreadingHost(this, context, TimeSpan.FromSeconds(1 / RenderFrequency));
			renderHost.Listen();
			
			audioHost = new GameAudioThreadingHost(context, TimeSpan.FromSeconds(1f / 1000));
			audioHost.Listen();

			disposables.Add(renderHost);
			disposables.Add(audioHost);
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			GL.Viewport(0, 0, e.Width, e.Height);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}