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
			return !World.Mgr.Get<VisualHWND>().IsEmpty && !World.Mgr.Get<GameClient>().IsEmpty && base.CanUpdate();
		}

		private IntPtr visualHwnd;
		protected override void OnUpdate()
		{
			base.OnUpdate();

			visualHwnd = World.Mgr.Get<VisualHWND>()[0].Value;
			//EnableWindow(visualHwnd, false);
			foreach (var gameClient in World.Mgr.Get<GameClient>())
			{
				if (!gameClient.IsHwndIntegrated)
					continue;

				if (gameClient.Hwnd != IntPtr.Zero)
				{
					SetWindowPos(gameClient.Hwnd, IntPtr.Zero, 100, 0, 512, 512, (uint) 0x0200);
				}

				if (gameClient.IsHwndInFront)
					continue;

				EnumChildWindows(visualHwnd, (hwnd, lparam) =>
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
			SetWindowPos(hwnd, visualHwnd, 0, 0, 512, 512, (uint) 0x0200);
			EnableWindow(hwnd, true);
			EnableWindow(hwnd, false);
			EnableWindow(visualHwnd, true);
			SetFocus(visualHwnd);
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
	}
}