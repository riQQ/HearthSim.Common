using System;
using System.IO;
using System.Linq;
using HearthSim.Core.LogReading;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Util.EventArgs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.MockData
{
	public class MockLogInput : ILogInput
	{
		public void Read(string file)
		{
			if(NewLines == null)
				Assert.Fail("NewLines has no listeners");
			var lines = File.ReadLines(file).Select(x => new Line("Power", x));
			NewLines.Invoke(new NewLinesEventArgs(lines));
		}

		public event Action<NewLinesEventArgs> NewLines;
	}
}
