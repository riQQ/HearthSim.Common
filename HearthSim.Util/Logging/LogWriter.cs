using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HearthSim.Util.Logging
{
	internal class LogWriter
	{
		private readonly string _directory;
		private readonly string _baseFileName;
		private readonly int _numOldLogFiles;
		private readonly int _maxAgeDays;
		private static string _prevLine;
		private static int _duplicateCount;

		/// <exception cref="LogWriterCreationException"></exception>
		public LogWriter(string directory, string baseFileName, int numOldLogFiles = 25, int maxAgeDays = 2)
		{
			_directory = directory;
			_baseFileName = baseFileName;
			_numOldLogFiles = numOldLogFiles;
			_maxAgeDays = maxAgeDays;
			var logFile = Path.Combine(directory, baseFileName + ".txt");
			if(!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			else
			{
				try
				{
					if(File.Exists(logFile))
					{
						using(var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.None))
						{
							//can access log file => no other instance of same installation running
						}
						File.Move(logFile, logFile.Replace(".txt", "_" + DateTime.Now.ToFileTime() + ".txt"));
					}
					else
						File.Create(logFile).Dispose();
				}
				catch(Exception ex)
				{
					throw new LogWriterCreationException("Can not access log file", ex);
				}
			}
			DeleteOldLogs();
			try
			{
				Trace.Listeners.Add(new TextWriterTraceListener(new StreamWriter(logFile, false)));	
			}
			catch (Exception ex)
			{
				throw new LogWriterCreationException("Can not attack trace listener", ex);
			}
		}

		private void DeleteOldLogs()
		{
			try
			{
				var logFiles = new DirectoryInfo(_directory).GetFiles(_baseFileName + "*")
					.Where(x => x.LastWriteTime < DateTime.Now.AddDays(-_maxAgeDays))
					.OrderByDescending(x => x.LastWriteTime)
					.Skip(_numOldLogFiles);
				foreach(var file in logFiles)
					File.Delete(file.FullName);
			}
			catch(Exception)
			{
			}
		}

		public void WriteLine(string msg, LogType type, string memberName = "", string sourceFilePath = "")
		{
			var file = sourceFilePath?.Split('/', '\\').LastOrDefault()?.Split('.').FirstOrDefault();
			var line = $"{type}|{file}.{memberName} >> {msg}";

			if(line == _prevLine)
				_duplicateCount++;
			else
			{
				if(_duplicateCount > 0)
					Write($"... {_duplicateCount} duplicate messages");
				_prevLine = line;
				_duplicateCount = 0;
				Write(line);
			}
		}

		private void Write(string line) => Trace.WriteLine($"{DateTime.Now.ToLongTimeString()}|{line}");
	}
}
