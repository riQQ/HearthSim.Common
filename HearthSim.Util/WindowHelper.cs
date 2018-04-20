using System;
using System.Windows;

namespace HearthSim.Util
{
	public static class WindowHelper
	{
		private const int GwlExstyle = -20;
		private const int WsExTopmost = 0x00000008;
		private const int GwlStyle = -16;
		private const int WsMinimize = 0x20000000;
		private const int WsMaximize = 0x1000000;

		public const int SwRestore = 9;
		public const int SwShow = 5;

		private const int Alt = 0xA4;
		private const int ExtendedKey = 0x1;
		private const int KeyUp = 0x2;

		public static void SetWindowExStyle(IntPtr hwnd, int style)
		{
			NativeMethods.SetWindowLong(hwnd, GwlExstyle, NativeMethods.GetWindowLong(hwnd, GwlExstyle) | style);
		}

		public static bool IsTopmost(IntPtr hwnd)
		{
			return (NativeMethods.GetWindowLong(hwnd, GwlExstyle) & WsExTopmost) != 0;
		}

		public static bool IsInForeground(IntPtr hwnd)
		{
			return NativeMethods.GetForegroundWindow() == hwnd;
		}

		public static WindowState GetState(IntPtr hwnd)
		{
			var state = NativeMethods.GetWindowLong(hwnd, GwlStyle);
			if((state & WsMaximize) == WsMaximize)
				return WindowState.Maximized;
			if((state & WsMinimize) == WsMinimize)
				return WindowState.Minimized;
			return WindowState.Normal;
		}

		//http://www.roelvanlisdonk.nl/?p=4032
		public static void Activate(IntPtr hwnd)
		{
			if(hwnd == IntPtr.Zero)
				return;
			if(hwnd == NativeMethods.GetForegroundWindow())
				return;
			NativeMethods.ShowWindow(hwnd, GetState(hwnd) == WindowState.Minimized ? SwRestore : SwShow);
			NativeMethods.keybd_event(Alt, 0x45, ExtendedKey | 0, 0);
			NativeMethods.keybd_event(Alt, 0x45, ExtendedKey | KeyUp, 0);
			NativeMethods.SetForegroundWindow(hwnd);
		}

		public static void SetStyle(IntPtr hwnd, WndStyle style)
		{
			SetWindowExStyle(hwnd, (int)style);
		}
	}
}
