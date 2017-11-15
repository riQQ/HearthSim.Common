#region

using System.Collections.Generic;
using HearthSim.Core.Util.Logging;

#endregion

namespace HearthSim.Core.LogConfig
{
	internal class LogConfig
	{
		public bool Updated { get; private set; }

		public List<LogConfigItem> Items { get; } = new List<LogConfigItem>();

		public void Add(LogConfigItem configItem)
		{
			Log.Debug($"Adding {configItem.Name}");
			Items.Add(configItem);
			Updated = true;
		}

		public void Verify()
		{
			foreach(var item in Items)
				Updated |= item.VerifyAndUpdate();
		}
	}
}
