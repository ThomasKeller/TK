using System;
using System.Net;
using TK.Logging;
namespace TK.WebPageParser
{
	public class WebPagerReader
	{
		private static ILogger _logger = LoggerFactory.CreateLoggerFor(typeof(WebPagerReader));
		public static string Download(string url, string user = "", string password = "")
		{
			try
			{
				WebPagerReader._logger.DebugFormat("Download: Url: {0} User: {1}", new object[]
				{
					url,
					user
				});
				WebClient webclient = new WebClient();
				if (!string.IsNullOrEmpty(user))
				{
					WebPagerReader._logger.Debug("Download with credentials");
					webclient.Credentials = new NetworkCredential(user, password);
				}
				return webclient.DownloadString(new Uri(url));
			}
			catch (Exception ex)
			{
				WebPagerReader._logger.Error("Download failed: ", ex);
			}
			return string.Empty;
		}
	}
}
