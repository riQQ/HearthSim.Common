using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.Util.Analytics
{
	public class HttpInfluxClient : InfluxClient
	{
		private readonly string _url;

		public HttpInfluxClient(string url)
		{
			_url = url;
		}

		public override async Task Send(InfluxPoint point)
		{
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(_url);
				request.ContentType = "text/plain";
				request.Method = "POST";
				using(var stream = await request.GetRequestStreamAsync())
				{
					var line = point.ToLineProtocol();
					Log.Debug(line);
					stream.Write(Encoding.UTF8.GetBytes(line), 0, line.Length);
					using(var response = (HttpWebResponse)await request.GetResponseAsync())
						Log.Debug(response.StatusCode.ToString());
				}
			}
			catch(Exception ex)
			{
				Log.Debug(ex.ToString());
			}
		}
	}
}
