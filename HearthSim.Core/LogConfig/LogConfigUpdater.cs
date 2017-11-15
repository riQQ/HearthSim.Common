using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HearthSim.Core.Util.Logging;
using static HearthSim.Core.LogConfig.LogConfigConstants;

namespace HearthSim.Core.LogConfig
{
	internal class LogConfigUpdater
	{
		private static bool _running;
		private static List<string> _requiredLogs;
		public static event Action LogConfigUpdated;
		public static event Action LogConfigUpdateFailed;

		public static async Task Run(IEnumerable<string> requiredLogs = null)
		{
			if(requiredLogs != null)
				_requiredLogs = requiredLogs.ToList();
			if(_running)
				return;
			_running = true;
			LogConfigWatcher.Pause();
			try
			{
				if(File.Exists(LogConfigPath))
					await WaitForFileAccess(LogConfigPath, 500);
				if(CheckLogConfig())
				{
					Log.Debug("log.config was updated");
					LogConfigUpdated?.Invoke();
				}
				else
					Log.Debug("log.config up to date");
			}
			catch(Exception e)
			{
				Log.Error(e);
				LogConfigUpdateFailed?.Invoke();
			}
			finally
			{
				LogConfigWatcher.Continue();
				_running = false;
			}
		}

		private static bool CheckLogConfig()
		{
			try
			{
				var logConfig = ReadLogConfig();
				var required = _requiredLogs ?? new List<string>();
				foreach(var log in required.Where(name => logConfig.Items.All(x => x.Name != name)))
					logConfig.Add(new LogConfigItem(log));
				logConfig.Verify();
				if(logConfig.Updated)
					WriteLogConfig(logConfig);
				return logConfig.Updated;
			}
			catch(Exception e)
			{
				Log.Error(e);
				throw;
			}
		}

		private static void WriteLogConfig(LogConfig logConfig)
		{
			try
			{
				// ReSharper disable once ObjectCreationAsStatement
				if(File.Exists(LogConfigPath))
					new FileInfo(LogConfigPath) {IsReadOnly = false};
			}
			catch(Exception e)
			{
				Log.Debug("Could not remove read-only from log.config:\n" + e);
			}
			Log.Debug("Updating log.config");
			using(var sw = new StreamWriter(LogConfigPath))
				sw.Write(string.Concat(logConfig.Items));
		}

		private static LogConfig ReadLogConfig()
		{
			var logConfig = new LogConfig();
			if(!File.Exists(LogConfigPath))
				return logConfig;
			using(var sr = new StreamReader(LogConfigPath))
			{
				LogConfigItem current = null;
				string line;
				while(!sr.EndOfStream && (line = sr.ReadLine()) != null)
				{
					var match = NameRegex.Match(line);
					if(match.Success)
					{
						current = new LogConfigItem(match.Groups["value"].Value);
						logConfig.Items.Add(current);
						continue;
					}
					if(current == null)
						continue;
					if(TryParseLine(line, LogLevelRegex, ref current.LogLevel))
						continue;
					if(TryParseLine(line, FilePrintingRegex, ref current.FilePrinting))
						continue;
					if(TryParseLine(line, ConsolePrintingRegex, ref current.ConsolePrinting))
						continue;
					if(TryParseLine(line, ScreenPrintingRegex, ref current.ScreenPrinting))
						continue;
					var verbose = false;
					if(TryParseLine(line, VerboseRegex, ref verbose))
						current.Verbose = verbose;
				}
			}
			return logConfig;
		}

		private static bool TryParseLine(string line, Regex regex, ref int value)
		{
			var match = regex.Match(line);
			if(!match.Success)
				return false;
			value = int.Parse(match.Groups["value"].Value);
			return true;
		}

		private static bool TryParseLine(string line, Regex regex, ref bool value)
		{
			var match = regex.Match(line);
			if(!match.Success)
				return false;
			if(bool.TryParse(match.Groups["value"].Value, out var boolValue))
			{
				value = boolValue;
				return true;
			}
			if(int.TryParse(match.Groups["value"].Value, out var intValue))
			{
				value = intValue > 0;
				return true;
			}
			value = false;
			return true;
		}

		private static async Task WaitForFileAccess(string path, int delay)
		{
			while(true)
			{
				try
				{
					using(var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
					{
						if(stream.Name != null)
						break;
					}
				}
				catch
				{
					await Task.Delay(delay);
				}
			}
		}
	}
}
