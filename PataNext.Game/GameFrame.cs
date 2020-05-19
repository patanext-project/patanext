using System;
using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Core.Threading;
using GameHost.Injection;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;

namespace PataponGameHost
{	
	public class GameFrame : GameWindow
	{
		public class FrameListener : IFrameListener
		{
			private SimpleFrameListener super  = new SimpleFrameListener();
			private List<WorkerFrame>   frames = new List<WorkerFrame>();

			public bool Add(WorkerFrame frame)
			{
				return super.Add(frame);
			}

			public int LastCollectionIndex;
			public double Delta;
			
			public List<WorkerFrame> DequeueAll()
			{
				frames.Clear();
				super.DequeueAll(frames);
				foreach (var frame in frames)
				{
					if (frame.CollectionIndex != LastCollectionIndex)
					{
						Delta = 0;
						LastCollectionIndex = frame.CollectionIndex;
					}

					Delta = Math.Max(frame.Delta.TotalSeconds, Delta);
				}
				
				return frames;
			}
		}

		private Context context;

		private List<IDisposable> disposables;
		
		private GameRenderThreadingHost renderHost;
		private GameAudioThreadingHost audioHost;

		private FrameListener renderFrameListener;
		private FrameListener simulationFrameListener;
		private FrameListener inputFrameListener;
		
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
			
			renderHost = new GameRenderThreadingHost(this, context);
			renderHost.Listen();
			
			audioHost = new GameAudioThreadingHost(context, TimeSpan.FromSeconds(1f / 100));
			audioHost.Listen();

			disposables.Add(renderHost);
			disposables.Add(audioHost);

			//this.WindowState = WindowState.Fullscreen;
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			GL.Viewport(0, 0, e.Width, e.Height);
		}

		protected override unsafe void OnRenderFrame(FrameEventArgs args)
		{
			void setListener<T>(ref FrameListener listener)
				where T : GameThreadedHostApplicationBase<T>
			{
				if (listener != null)
					return;

				if (!ThreadingHost.TypeToThread.TryGetValue(typeof(T), out var host))
					return;
				
				var client = (T) host.Host;

				using var threadLocker = client.SynchronizeThread();
				if (client.Worker != null)
					client.Worker.FrameListener.TryAdd(listener = new FrameListener());
			}
			
			setListener<GameSimulationThreadingHost>(ref simulationFrameListener);
			setListener<GameRenderThreadingHost>(ref renderFrameListener);
			setListener<GameInputThreadingHost>(ref inputFrameListener);

			simulationFrameListener.DequeueAll();
			renderFrameListener.DequeueAll();
			
			Title = $"PataNext (Render, Frame={renderFrameListener.Delta:0.00}ms) (Simulation, Frame={simulationFrameListener.Delta:0.00}ms) (Input, Frame={inputFrameListener.Delta:0.00}ms)";
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}