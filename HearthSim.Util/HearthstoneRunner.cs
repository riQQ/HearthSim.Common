using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HearthSim.Util.Logging;

namespace HearthSim.Util
{
	public static class HearthstoneRunner
	{
		public enum State
		{
			Starting,
			Success,
			Error
		}

		private const int ProcCheckInterval = 500;
		private const int ProcExistDuration = 7000;
		private const int TryStartBnetDuration = 20000;
		private const int StartBnetTries = TryStartBnetDuration / ProcCheckInterval;
		private const int EventDelay = 2000;

		private static bool _starting;
		private static readonly string[] ProcNames = { "Battle.net", "Battle.net.beta" };
		public static Action<State> StartingHearthstone;

		public static async Task StartHearthstone()
		{
			if(_starting || HearthstoneWindow.Get() != IntPtr.Zero)
				return;
			_starting = true;
			var result = State.Error;
			StartingHearthstone?.Invoke(State.Starting);
			try
			{
				var bnetProc = GetProcess();
				if(bnetProc == null)
				{
					var foundDuration = 0;
					Process.Start("battlenet://");
					for(var i = 0; i < StartBnetTries; i++)
					{
						bnetProc = GetProcess();
						if(bnetProc != null && bnetProc.MainWindowHandle != IntPtr.Zero)
							if((foundDuration += ProcCheckInterval) >= ProcExistDuration)
								break;
						await Task.Delay(ProcCheckInterval);
					}

					if(foundDuration == 0)
						return;
				}

				Process.Start("battlenet://WTCG");
				result = State.Success;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			finally
			{
				_starting = false;
				await Task.Delay(EventDelay);
				StartingHearthstone?.Invoke(result);
			}
		}

		private static Process GetProcess()
		{
			return ProcNames.Select(x => Process.GetProcessesByName(x).FirstOrDefault()).FirstOrDefault(x => x != null);
		}
	}
}
