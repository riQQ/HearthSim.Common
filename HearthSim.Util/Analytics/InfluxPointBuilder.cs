using System;
using System.Collections.Generic;

namespace HearthSim.Util.Analytics
{
	public class InfluxPointBuilder
	{
		private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();
		private readonly Dictionary<string, object> _tags = new Dictionary<string, object>();
		private readonly string _name;
		private DateTime? _timestamp;

		public InfluxPointBuilder(string name)
		{
			_name = name;
			_fields.Add("count", 1);
		}

		public InfluxPointBuilder Tag(string name, object value)
		{
			_tags.Add(name, value);
			return this;
		}

		public InfluxPointBuilder Field(string name, object value)
		{
			_fields.Add(name, value);
			return this;
		}

		public InfluxPointBuilder Timestamp(DateTime timestamp)
		{
			_timestamp = timestamp;
			return this;
		}

		public InfluxPoint Build()
		{
			return new InfluxPoint(_name, _tags, _fields, _timestamp ?? DateTime.UtcNow);
		}
	}
}
