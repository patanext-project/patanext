using System;
using System.Runtime.InteropServices;
using GameHost.Applications;
using GameHost.Core.Client;
using GameHost.Core.Ecs;
using PataNext.Export.Desktop.Visual;

namespace PataNext.Export.Desktop
{
	// big mess
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class BringIntegratedClientInFrontSystem : AppSystem
	{
		public BringIntegratedClientInFrontSystem(WorldCollection collection) : base(collection)
		{
		}

		public override bool CanUpdate()
		{
			var val = !World.Mgr.Get<VisualHWND>().IsEmpty && !World.Mgr.Get<GameClient>().IsEmpty && base.CanUpdate();
			if (!val)
			{
				timeBeforeEnablingMainWindow = Environment.TickCount + 500;
				return false;
			}

			return true;
		}

		private VisualHWND visualHwnd;
		private int timeBeforeEnablingMainWindow;
		
		protected override void OnUpdate()
		{
			base.OnUpdate();
			return; // quite buggy

			if (timeBeforeEnablingMainWindow > 0 
			    && timeBeforeEnablingMainWindow < Environment.TickCount)
			{
				timeBeforeEnablingMainWindow = -1;
				SetFocus(IntPtr.Zero);
				EnableWindow(visualHwnd.Value, false);
				return;
			}

			visualHwnd = World.Mgr.Get<VisualHWND>()[0];
			foreach (var gameClient in World.Mgr.Get<GameClient>())
			{
				if (!gameClient.IsHwndIntegrated && !visualHwnd.ShowIntegratedWindows)
					continue;

				if (gameClient.Hwnd != IntPtr.Zero && gameClient.IsHwndInFront)
				{
					if (visualHwnd.ShowIntegratedWindows)
					{
						SetWindowPos(gameClient.Hwnd, IntPtr.Zero, 0, 0, visualHwnd.Size.X, visualHwnd.Size.Y, (uint) 0);
						SetFocus(IntPtr.Zero);
						SetFocus(visualHwnd.Value);
						EnableWindow(visualHwnd.Value, true);
					}
					else
					{
						DeactivateClientWindow(gameClient.Hwnd);
						SetWindowPos(gameClient.Hwnd, IntPtr.Zero, -visualHwnd.Size.X, 0, visualHwnd.Size.X, visualHwnd.Size.Y, (uint) 0);
						SetFocus(visualHwnd.Value);
						gameClient.IsHwndInFront = false;
					}
				}

				if (gameClient.IsHwndInFront || !visualHwnd.ShowIntegratedWindows)
					continue;

				EnumChildWindows(visualHwnd.Value, (hwnd, lparam) =>
				{
					gameClient.Hwnd          = hwnd;
					gameClient.IsHwndInFront = true;
					ActivateClientWindow(gameClient.Hwnd);
					return 0;
				}, IntPtr.Zero);
			}
		}

		private void ActivateClientWindow(IntPtr hwnd)
		{
			Console.WriteLine("Active Window");
			SetWindowPos(hwnd, visualHwnd.Value, 0, 0, visualHwnd.Size.X, visualHwnd.Size.Y, (uint) 0x0200);
			EnableWindow(hwnd, true);
			EnableWindow(visualHwnd.Value, false);
			SetFocus(visualHwnd.Value);
			SendMessage(hwnd, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
		}

		private void DeactivateClientWindow(IntPtr hwnd)
		{
			SendMessage(hwnd, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
		}

		private const    int    WM_ACTIVATE = 0x0006;
		private readonly IntPtr WA_ACTIVE   = new IntPtr(1);
		private readonly IntPtr WA_INACTIVE = new IntPtr(0);
		
		[DllImport("User32.dll")]
		static extern bool SetWindowPos(IntPtr hdwd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
		[DllImport("User32.dll")]
		static extern bool EnableWindow(IntPtr hdwd, bool value);
		[DllImport("User32.dll")]
		static extern IntPtr SetFocus(IntPtr hdwn);

		internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
		[DllImport("user32.dll")]
		internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
		
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int msg);
	}
}