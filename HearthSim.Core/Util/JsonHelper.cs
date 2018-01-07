using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace HearthSim.Core.Util
{
	internal static class JsonHelper
	{
		public static IEnumerable<JProperty> GetChildren(JToken obj)
		{
			return obj.Children().OfType<JProperty>().Where(x => x.Values().Any());
		}
	}
}
