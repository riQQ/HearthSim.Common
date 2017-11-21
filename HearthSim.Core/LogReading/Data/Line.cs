using System;
using System.Text.RegularExpressions;

namespace HearthSim.Core.LogReading.Data
{
	public class Line
	{
		private static readonly Regex LineRegex = new Regex(@"^(D|I|W) (?<ts>([\d:.]+)) (?<text>(.*))$");

		public Line(string logName, string line)
		{
			LogName = logName;
			RawLine = line;
			Time = DateTime.Now;

			var match = LineRegex.Match(line);
			if(match.Success)
			{
				Text = match.Groups["text"].Value;
				if(DateTime.TryParse(match.Groups["ts"].Value, out var time))
				{
					Time = DateTime.Today.Add(time.TimeOfDay);
					if(Time > DateTime.Now)
						Time = Time.AddDays(-1);
				}
			}
			IsValid = match.Success;
		}

		public string LogName { get; }
		public string RawLine { get; }
		public string Text { get; }
		public DateTime Time { get; }
		public bool IsValid { get; }
	}
}
