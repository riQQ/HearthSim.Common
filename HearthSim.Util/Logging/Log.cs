#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#endregion

namespace HearthSim.Util.Logging
{
	[DebuggerStepThrough]
	public class Log
	{
		private static readonly Queue<Tuple<string, LogType, string, string>> LogQueue = new Queue<Tuple<string, LogType, string, string>>();
		private static LogWriter _logWriter;

		public static event Action<ErrorEventArgs> EventLogged;

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
			[CallerFilePath] string sourceFilePath = "", bool invokeEvent = true)
		{
			if(_logWriter == null)	
				LogQueue.Enqueue(new Tuple<string, LogType, string, string>(msg, logType, memberName, sourceFilePath));
			else
				_logWriter.WriteLine(msg, logType, memberName, sourceFilePath);
			if(invokeEvent)
				EventLogged?.Invoke(new ErrorEventArgs(logType, msg, sourceFilePath, memberName));
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
		{
			WriteLine(ex.ToString(), LogType.Error, memberName, sourceFilePath, false);
			EventLogged?.Invoke(new ErrorEventArgs(LogType.Error, ex, sourceFilePath, memberName));
		}

		public static void Fatal(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
		{
			WriteLine(ex.ToString(), LogType.Fatal, memberName, sourceFilePath, false);
			EventLogged?.Invoke(new ErrorEventArgs(LogType.Fatal, ex, sourceFilePath, memberName));
		}
	}

	public class ErrorEventArgs : System.EventArgs
	{
		public ErrorEventArgs(LogType type, string message, string sourceFilePath, string memberName)
		{
			Type = type;
			Message = message;
			SourceFilePath = sourceFilePath;
			MemberName = memberName;
		}

		public ErrorEventArgs(LogType type, Exception exception, string sourceFilePath, string memberName)
		{
			Type = type;
			Exception = exception;
			SourceFilePath = sourceFilePath;
			MemberName = memberName;
		}

		public Exception Exception { get; }
		public LogType Type { get; }
		public string Message { get; }
		public string SourceFilePath { get; }
		public string MemberName { get; }
	}
}
