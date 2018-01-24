using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HearthSim.Util.Logging;

namespace HearthSim.Util.Analytics
{
	public class UdpInfluxClient : InfluxClient
	{
		private readonly string _hostname;
		private readonly int _port;

		public UdpInfluxClient(string hostname, int port)
		{
			_hostname = hostname;
			_port = port;
		}

		public override async Task Send(InfluxPoint point)
		{
			try
			{
				using(var client = new UdpClient())
				{
					var line = point.ToLineProtocol();
					var data = Encoding.UTF8.GetBytes(line);
					var length = await client.SendAsync(data, data.Length, _hostname, _port);
					Log.Debug(line + " - " + length);
				}
			}
			catch(Exception ex)
			{
				Log.Debug(ex.ToString());
			}
		}
	}
}
