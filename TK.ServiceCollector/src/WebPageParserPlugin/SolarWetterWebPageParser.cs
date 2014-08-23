using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TK.PluginManager;
using TK.WebPageParser;

namespace TK.WebPageParserPlugin
{
    public class SolarWetterWebPageParser
    {
        public static MeasureValueBox Parse(string webPage)
        {
            var result = new SortedDictionary<string, object>();
            var measuredValueBox = new MeasureValueBox() { MeasuredUtcTime = DateTime.UtcNow, PluginName = "WebPageParser" };
            var arrayParser = new ArrayParser(WebPagerReader.SelectAllElementsOfName(webPage, "td").ToArray());
            result.Add("SW.MeasureTime", DateTime.Now);
            arrayParser.MoveToNextElementNotEmpty(0, true);
            arrayParser.MoveNext();
            arrayParser.MoveToNextElementNotEmpty(0, true);
            arrayParser.MoveNext();
            string all = arrayParser.MoveToNextElementNotEmpty(0, true);
            arrayParser.MoveNext();
            result.Add("SW.All", all);
            var cultureInfo = new CultureInfo("de");
            result.Add("SW.ClearSky", ArrayParser.ConvertToDouble(arrayParser.SearchForExactElement("clear sky:", 1, true), cultureInfo));
            result.Add("SW.RealSkyMin", ArrayParser.ConvertToDouble(arrayParser.SearchForExactElement("real sky:", 1, false), cultureInfo));
            result.Add("SW.RealSkyMax", ArrayParser.ConvertToDouble(arrayParser.SearchForExactElement("real sky:", 3, false), cultureInfo));
            measuredValueBox.MeasuredValues = result;
            return measuredValueBox;
        }
    }
}