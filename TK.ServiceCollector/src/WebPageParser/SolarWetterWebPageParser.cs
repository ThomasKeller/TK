using AngleSharp;
using AngleSharp.DOM;
using AngleSharp.DOM.Collections;
using AngleSharp.DOM.Html;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace TK.WebPageParser
{
	public class SolarWetterWebPageParser
	{
		public static IDictionary<string, object> Parse(string webPage)
		{
			SortedDictionary<string, object> result = new SortedDictionary<string, object>();
			var document = DocumentBuilder.Html(webPage, null);
			var elements = document.QuerySelectorAll("td");
			IEnumerable<string> tdArray = 
				from text in elements
				select text.TextContent;
			ArrayParser arrayParser = new ArrayParser(tdArray.ToArray<string>());
			result.Add("SW.MeasureTime", DateTime.Now);
			arrayParser.MoveToNextElementNotEmpty(0, true);
			arrayParser.MoveNext();
			arrayParser.MoveToNextElementNotEmpty(0, true);
			arrayParser.MoveNext();
			string all = arrayParser.MoveToNextElementNotEmpty(0, true);
			arrayParser.MoveNext();
			result.Add("SW.All", all);
			CultureInfo cultureInfo = new CultureInfo("de");
			result.Add("SW.ClearSky", ArrayParser.ConvertToDouble(arrayParser.SearchForExactElement("clear sky:", 1, true), cultureInfo));
			result.Add("SW.RealSkyMin", ArrayParser.ConvertToDouble(arrayParser.SearchForExactElement("real sky:", 1, false), cultureInfo));
			result.Add("SW.RealSkyMax", ArrayParser.ConvertToDouble(arrayParser.SearchForExactElement("real sky:", 3, false), cultureInfo));
			return result;
		}
	}
}
