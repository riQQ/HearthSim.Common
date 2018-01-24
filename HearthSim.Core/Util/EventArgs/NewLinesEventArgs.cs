using System.Collections.Generic;
using System.Linq;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.Util.EventArgs
{
	public class NewLinesEventArgs : System.EventArgs
	{
		public NewLinesEventArgs(IEnumerable<Line> lines)
		{
			Lines = lines.ToList();
		}

		public List<Line> Lines { get; }
	}
}
