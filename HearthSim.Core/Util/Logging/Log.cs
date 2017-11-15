#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#endregion

namespace HearthSim.Core.Util.Logging
{
	[DebuggerStepThrough]
	public class Log
	{
		private static readonly Queue<Tuple<string, LogType, string, string>> LogQueue = new Queue<Tuple<string, LogType, string, string>>();
		private static LogWriter _logWriter;

		public static bool Initialize(string directory, string baseFileName, int numOldLogFiles = 25, int maxAgeDays = 2)
		{
			if(_logWriter != null)
				return false;
			Trace.AutoFlush = true;
			try
			{
				_logWriter = new LogWriter(directory, baseFileName, numOldLogFiles, maxAgeDays);
			}
			catch(Exception)
			{
				return false;
			}
			foreach(var line in LogQueue)
				Trace.WriteLine(line);
			return true;
		}

		private static void WriteLine(string msg, LogType logType, [CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "")
		{
			if(_logWriter == null)	
				LogQueue.Enqueue(new Tuple<string, LogType, string, string>(msg, logType, memberName, sourceFilePath));
			else
				_logWriter.WriteLine(msg, logType, memberName, sourceFilePath);
		}

		public static void Debug(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Debug, memberName, sourceFilePath);

		public static void Info(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Info, memberName, sourceFilePath);

		public static void Warn(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Warning, memberName, sourceFilePath);

		public static void Error(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Error, memberName, sourceFilePath);

		public static void Error(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(ex.ToString(), LogType.Error, memberName, sourceFilePath);
	}
}
