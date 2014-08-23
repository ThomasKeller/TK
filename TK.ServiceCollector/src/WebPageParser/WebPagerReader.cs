using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using AngleSharp;
using TK.Logging;

namespace TK.WebPageParser
{
    public class WebPagerReader
    {
        private static ILogger _logger = LoggerFactory.GetCurrentClassLogger();

        public static string Download(string url, string user = "", string password = "")
        {
            try
            {
                _logger.DebugFormat("Download: Url: {0} User: {1}", url, user);
                WebClient webclient = new WebClient();
                if (!string.IsNullOrEmpty(user))
                {
                    _logger.Debug("Download with credentials");
                    webclient.Credentials = new NetworkCredential(user, password);
                }
                return webclient.DownloadString(new Uri(url));
            }
            catch (Exception ex)
            {
                _logger.Error("Download failed: ", ex);
            }
            return string.Empty;
        }

        public static IEnumerable<string> SelectAllElementsOfName(string webPage, string typeText = "td")
        {
            var document = DocumentBuilder.Html(webPage, null);
            var elements = document.QuerySelectorAll(typeText);
            IEnumerable<string> result =
                from text in elements
                select text.TextContent;
            return result;
        }
    }
}