using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Game;
using GameHost.Entities;
using GameHost.Event;
using GameHost.Injection;
using GameHost.UI;
using GameHost.UI.Noesis;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;

namespace PataponGameHost
{	
	public class GameFrame : GameWindow
	{
		public readonly World World;
		
		private Context context;
		private GameRenderThreadingHost renderHost;
		private GameAudioThreadingHost audioHost;
		
		public GameFrame(Context context) : base(new GameWindowSettings
		{
			IsMultiThreaded = false,
			RenderFrequency = 500,
			UpdateFrequency = 500
		}, new NativeWindowSettings
		{
			Title = "PataNext.Game",
			Size  = new OpenToolkit.Mathematics.Vector2i {X = 1280, Y = 720},
			API   = ContextAPI.OpenGL,
		})
		{
			this.context = context;
			
			VSync = VSyncMode.Off;
			World = new World();
		}

		protected override unsafe void OnLoad()
		{
			base.OnLoad();
			
			GL.LoadBindings(new GLFWBindingsContext());
			GLFW.MakeContextCurrent(null);
			
			renderHost = new GameRenderThreadingHost(this, context);
			renderHost.Listen();
			
			audioHost = new GameAudioThreadingHost(context, TimeSpan.FromSeconds(1f / 100));
			audioHost.Listen();
		}

		protected override void OnClosing(CancelEventArgs args)
		{
			base.OnClosing(args);
			renderHost.Dispose();
			audioHost.Dispose();
		}

		private const string LibraryName = "glfw3-x64.dll";
		[DllImport(LibraryName)]
		public static extern unsafe int glfwGetWin32Window(OpenToolkit.Windowing.GraphicsLibraryFramework.Window* window);

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			GL.Viewport(0, 0, e.Width, e.Height);
		}

		protected override unsafe void OnRenderFrame(FrameEventArgs args)
		{
			Title = $"PataNext.Game (Vsync: {VSync}) FPS: {1f / args.Time:0} (input: {GamePerformance.GetFps("input")}FPS) (sim: {GamePerformance.GetFps("simulation")}FPS)";

			/*base.OnRenderFrame(args);

			gui.Update((DateTime.Now - startTime).TotalSeconds);
			gui.PrepareRender();

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			GL.ClearDepth(1.0f);
			GL.DepthMask(true);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.StencilTest);
			GL.Disable(EnableCap.Blend);
			GL.Disable(EnableCap.ScissorTest);

			GL.UseProgram(0);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.Viewport(0, 0, Size.X, Size.Y);
			GL.ColorMask(true, true, true, true);

			GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			gui.Render();

			SwapBuffers();*/
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}