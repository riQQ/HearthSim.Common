using System;
using HearthSim.Core.LogReading.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.LogReading
{
	[TestClass]
	public class LineTest
	{
		private const string LineContent = "foo bar";
		private const string LogLineGeneric = "D 12:47:04.6578486 " + LineContent;
		private const string LogLineWarning = "W 12:47:04.6578486 " + LineContent;
		private const string LogLineEndOfDay = "D 24:59:59.9999999 " + LineContent;

		[TestMethod]
		public void Timestamp_Parsed()
		{
			var line = new Line("Power", LogLineGeneric);
			Assert.AreEqual(12, line.Time.Hour);
			Assert.AreEqual(47, line.Time.Minute);
			Assert.AreEqual(4, line.Time.Second);
			Assert.AreEqual(657, line.Time.Millisecond);
		}

		public void Timestamp_FutureTime_ParsedAsYesterday()
		{
			var line = new Line("Power", LogLineEndOfDay);
			Assert.AreEqual(DateTime.Now.AddDays(-1), line.Time.Day);
		}

		public void LineContent_Parsed()
		{
			var line = new Line("Power", LogLineGeneric);
			Assert.AreEqual(LineContent, line.Text);
		}

		[TestMethod]
		public void DataStored()
		{
			var line = new Line("Power", LogLineGeneric);
			Assert.AreEqual("Power", line.LogName);
			Assert.AreEqual(LogLineGeneric, line.RawLine);
		}
	}
}
