using System;
using System.Runtime.InteropServices;
using System.Threading;
using GameHost.Applications;
using GameHost.Core.Client;
using GameHost.Core.Ecs;
using PataNext.Export.Desktop.Visual;

namespace PataNext.Export.Desktop
{
	// big mess
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	[DontInjectSystemToWorld] // don't work on linux
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
				EnableWindow(visualHwnd.Value, true);
				
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

			// quite buggy
			/*if (timeBeforeEnablingMainWindow > 0 
			    && timeBeforeEnablingMainWindow < Environment.TickCount)
			{
				timeBeforeEnablingMainWindow = -1;
				SetFocus(IntPtr.Zero);
				EnableWindow(visualHwnd.Value, false);
				return;
			}*/

			visualHwnd = World.Mgr.Get<VisualHWND>()[0];
			foreach (var gameClient in World.Mgr.Get<GameClient>())
			{
				if (!gameClient.IsHwndIntegrated && !visualHwnd.ShowIntegratedWindows)
					continue;

				if (gameClient.Hwnd != IntPtr.Zero && gameClient.IsHwndInFront)
				{
					if (visualHwnd.ShowIntegratedWindows)
					{
						SetWindowPos(gameClient.Hwnd, IntPtr.Zero, 0, 0,visualHwnd.Size.X, visualHwnd.Size.Y, (uint) 0);
						SetFocus(IntPtr.Zero);
						SetFocus(visualHwnd.Value);
						EnableWindow(visualHwnd.Value, true);
						
						SendMessage(gameClient.Hwnd, WM_ACTIVATE, WA_ACTIVE, 0);
						SendMessage(visualHwnd.Value, WM_ACTIVATE, WA_INACTIVE, 0);
					}
					else
					{
						DeactivateClientWindow(gameClient.Hwnd);
						//SetWindowPos(gameClient.Hwnd, IntPtr.Zero, -visualHwnd.Size.X, 0, visualHwnd.Size.X, visualHwnd.Size.Y, (uint) 0x0010);
						ShowWindow(gameClient.Hwnd, 0);
						ShowWindow(visualHwnd.Value, 0);
						EnableWindow(visualHwnd.Value, true);
						gameClient.IsHwndInFront = false;
						
						SendMessage(visualHwnd.Value, WM_ACTIVATE, WA_ACTIVE, 0);

						Console.WriteLine("disable!");
					}
				}

				if (gameClient.IsHwndInFront || !visualHwnd.ShowIntegratedWindows)
					continue;

				Thread.Sleep(100);
				
				EnumChildWindows(visualHwnd.Value, (hwnd, lparam) =>
				{
					gameClient.Hwnd          = hwnd;
					gameClient.IsHwndInFront = true;
					/*ActivateClientWindow(gameClient.Hwnd);
					EnableWindow(gameClient.Hwnd, true);
					SetFocus(hwnd);*/
					//ActivateClientWindow(hwnd);
					SendMessage(hwnd, 6, 1, 0);
					return 0;
				}, IntPtr.Zero);
				
				SetWindowPos(gameClient.Hwnd, IntPtr.Zero, 0, 0, 10, 10, (uint) 0);
				ActivateClientWindow(gameClient.Hwnd);
				EnableWindow(gameClient.Hwnd, true);
				SetFocus(gameClient.Hwnd);

				SetWindowPos(gameClient.Hwnd, IntPtr.Zero, 0, 0, 10, 10, (uint) 0);
				ShowWindow(gameClient.Hwnd, 1);
			}
		}

		private void ActivateClientWindow(IntPtr hwnd)
		{
			Console.WriteLine("Active Window");
			SetWindowPos(hwnd, visualHwnd.Value, 0, 0, visualHwnd.Size.X, visualHwnd.Size.Y, (uint) 0x0200);
			EnableWindow(hwnd, true);
			EnableWindow(visualHwnd.Value, false);
			SetFocus(visualHwnd.Value);
			SendMessage(hwnd, WM_ACTIVATE, WA_ACTIVE, 0);
		}

		private void DeactivateClientWindow(IntPtr hwnd)
		{
			SendMessage(hwnd, WM_ACTIVATE, WA_INACTIVE, 0);
		}

		public void ForceEnableMainWindow()
		{
			if (visualHwnd.Value == IntPtr.Zero)
				return;
			
			ShowWindow(visualHwnd.Value, 0);
			EnableWindow(visualHwnd.Value, true);
		}

		private const    int    WM_ACTIVATE = 0x0006;
		private readonly int WA_ACTIVE   = 1;
		private readonly int WA_INACTIVE = 0;
		
		[DllImport("User32.dll")]
		static extern bool SetWindowPos(IntPtr hdwd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
		[DllImport("User32.dll")]
		static extern bool EnableWindow(IntPtr hdwd, bool value);
		[DllImport("User32.dll")]
		static extern IntPtr SetActiveWindow (IntPtr hdwd);
		[DllImport("User32.dll")]
		static extern IntPtr SetFocus(IntPtr hdwn);

		internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
		[DllImport("user32.dll")]
		internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int msg);
		
		[DllImport("user32.dll")]
		static extern bool HideWindow(IntPtr hWnd, int msg);
	}
}