using System;
using System.Collections.Generic;
using System.Linq;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogReading
{
	internal class NewLinesEventArgs : EventArgs
	{
		public NewLinesEventArgs(IEnumerable<Line> lines)
		{
			Lines = lines.ToList();
		}

		public List<Line> Lines { get; }
	}
}
