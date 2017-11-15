using System;
using System.IO;
using System.Linq;
using System.Text;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogReading.Internal
{
	public class Analyzer
	{
		private const int ChunkSize = 4096;

		public static long GetOffset(string filePath, DateTime startingPoint)
		{
			var fileInfo = new FileInfo(filePath);
			if(!fileInfo.Exists)
				return 0;
			using(var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using(var sr = new StreamReader(fs, Encoding.ASCII))
			{
				var offset = 0;
				while(offset < fs.Length)
				{
					var sizeDiff = ChunkSize - Math.Min(fs.Length - offset, ChunkSize);
					offset += ChunkSize;
					var chunk = ReadChunk(offset, fs, sr);
					var incomplete = chunk.IndexOf('\n');
					offset -= incomplete;
					var lines = chunk.Substring(incomplete, chunk.Length - incomplete).Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToArray();
					for(var i = lines.Length - 1; i > 0; i--)
					{
						if(string.IsNullOrWhiteSpace(lines[i].Trim('\0')))
							continue;
						var logLine = new Line("", lines[i]);
						if(logLine.Time >= startingPoint)
							continue;
						var negativeOffset = lines.Take(i + 1).Sum(x => Encoding.UTF8.GetByteCount(x + Environment.NewLine));
						return Math.Max(fs.Length - offset + negativeOffset + sizeDiff, 0);
					}
				}
			}
			return 0;
		}

		public static DateTime FindEntryPoint(LogWatcher logWatcher)
		{
			if(string.IsNullOrEmpty(logWatcher?.FilePath) || !logWatcher.Info.EntryPoints.Any())
				return DateTime.MinValue;
			var fileInfo = new FileInfo(logWatcher.FilePath);
			if(!fileInfo.Exists)
				return DateTime.MinValue;
			var ids = logWatcher.Info.EntryPoints.Select(x => new string(x.Reverse().ToArray())).ToList();
			using(var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using(var sr = new StreamReader(fs, Encoding.ASCII))
			{
				var offset = 0;
				while(offset < fs.Length)
				{
					offset += ChunkSize;
					var chunk = ReadChunk(offset, fs, sr);
					var incomplete = chunk.IndexOf('\n');
					if(incomplete >= ChunkSize)
						continue;
					offset -= incomplete;
					var reverse = new string(chunk.Substring(incomplete, chunk.Length - incomplete).Reverse().ToArray());
					var idOffsets = ids.Select(x => reverse.IndexOf(x, StringComparison.Ordinal)).Where(i => i > -1).ToList();
					if(!idOffsets.Any())
						continue;
					var line = new string(reverse.Substring(idOffsets.Min()).TakeWhile(c => c != '\n').Reverse().ToArray());
					return new Line("", line).Time;
				}
			}
			return DateTime.MinValue;
		}

		private static string ReadChunk(int offset, Stream stream, TextReader reader)
		{
			var buffer = new char[ChunkSize];
			stream.Seek(Math.Max(stream.Length - offset, 0), SeekOrigin.Begin);
			reader.ReadBlock(buffer, 0, ChunkSize);
			return new string(buffer.ToArray());
		}
	}
}
