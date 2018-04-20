using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace HearthSim.Util
{
	public static class HearthstoneWindow
	{
		private static DateTime _lastCheck;
		private static IntPtr _hsWindow;

		private static readonly Dictionary<IntPtr, string> WindowNameCache = new Dictionary<IntPtr, string>();

		private static readonly string[] WindowNames = { "Hearthstone", "하스스톤", "《爐石戰記》", "炉石传说" };

		public static IntPtr Get(bool useAnyUnityWindow = false)
		{
			if(DateTime.Now - _lastCheck < new TimeSpan(0, 0, 5) && _hsWindow == IntPtr.Zero)
				return IntPtr.Zero;
			if(_hsWindow != IntPtr.Zero)
			{
				if(NativeMethods.IsWindow(_hsWindow))
					return _hsWindow;
				_hsWindow = IntPtr.Zero;
				WindowNameCache.Clear();
			}

			if(useAnyUnityWindow)
				return GetUnityWindow();
			foreach(var windowName in WindowNames)
			{
				_hsWindow = NativeMethods.FindWindow("UnityWndClass", windowName);
				if(_hsWindow != IntPtr.Zero)
					break;
			}

			_lastCheck = DateTime.Now;
			return _hsWindow;
		}

		public static bool Exists()
		{
			return Get() != IntPtr.Zero;
		}

		public static bool IsInForeground()
		{
			return NativeMethods.GetForegroundWindow() == Get();
		}

		public static WindowState GetState()
		{
			return WindowHelper.GetState(Get());
		}

		public static void Flash()
		{
			NativeMethods.FlashWindow(Get(), false);
		}

		public static void Activate()
		{
			WindowHelper.Activate(Get());
		}

		private static IntPtr GetUnityWindow()
		{
			foreach(var process in Process.GetProcesses())
			{
				var handle = process.MainWindowHandle;
				if(!WindowNameCache.TryGetValue(handle, out var name))
				{
					var sb = new StringBuilder(200);
					NativeMethods.GetClassName(handle, sb, 200);
					name = sb.ToString();
					if(!string.IsNullOrEmpty(name))
						WindowNameCache[handle] = name;
				}

				if(!name.Equals("UnityWndClass", StringComparison.InvariantCultureIgnoreCase))
					continue;
				_hsWindow = handle;
				_lastCheck = DateTime.Now;
				return _hsWindow;
			}

			_lastCheck = DateTime.Now;
			return IntPtr.Zero;
		}
	}
}
