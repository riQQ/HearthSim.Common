using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.LogReading.Internal
{
	internal class LogWatcher
	{
		private ConcurrentQueue<Line> _lines = new ConcurrentQueue<Line>();
		private bool _logFileExists;
		private long _offset;
		private bool _running;
		private DateTime _startingPoint;
		private bool _stop;
		private Thread _thread;

		public LogWatcher(LogWatcherData info)
		{
			Info = info;
		}

		public LogWatcherData Info { get; }

		public void Start(string directory, DateTime startingPoint)
		{
			if(_running)
				return;
			var filePath = Path.Combine(directory, Info.Name + ".log");
			if(Util.ArchiveFile(filePath))
				Log.Debug($"Moved {Info.Name}.log to {Info.Name}_old.log");
			_startingPoint = startingPoint;
			_stop = false;
			_offset = 0;
			_logFileExists = false;
			_thread = new Thread(() => ReadLogFile(filePath)) {IsBackground = true};
			_thread.Start();
		}

		public async Task Stop()
		{
			Log.Debug($"Stopping {Info.Name}.log reader");
			_stop = true;
			while(_running || _thread == null || _thread.ThreadState == ThreadState.Unstarted)
				await Task.Delay(50);
			_lines = new ConcurrentQueue<Line>();
			await Task.Factory.StartNew(() => _thread?.Join());
		}

		public IEnumerable<Line> Collect()
		{
			var count = _lines.Count;
			for(var i = 0; i < count; i++)
			{
				if(_lines.TryDequeue(out var line))
					yield return line;
			}
		}

		private void ReadLogFile(string filePath)
		{
			_running = true;
			_offset = Analyzer.GetOffset(filePath, _startingPoint);
			Log.Debug($"Waiting for {Info.Name}.log, offset={_offset}");
			while(!_stop)
			{
				var fileInfo = new FileInfo(filePath);
				if(fileInfo.Exists)
				{
					if(!_logFileExists)
					{
						_logFileExists = true;
						Log.Debug($"Found {Info.Name}.log");
					}
					using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						fs.Seek(_offset, SeekOrigin.Begin);
						if(fs.Length == _offset)
						{
							Thread.Sleep(LogReader.UpdateDelay);
							continue;
						}
						using(var sr = new StreamReader(fs))
						{
							string line;
							while(!sr.EndOfStream && (line = sr.ReadLine()) != null)
							{
								var next = sr.Peek();
								if(!sr.EndOfStream && !(next == 'D' || next == 'I' || next == 'W'))
									break;
								var logLine = new Line(Info.Name, line);
								if(logLine.IsValid)
								{
									if(logLine.Time >= _startingPoint && (!Info.HasFilters || Info.Filters.Any(x => x(logLine.Text))))
										_lines.Enqueue(logLine);
								}
								else
									Log.Debug($"Invalid {Info.Name}.log line ignored: {line}");
								_offset += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
							}
						}
					}
				}
				Thread.Sleep(LogReader.UpdateDelay);
			}
			_running = false;
		}

	}
}
