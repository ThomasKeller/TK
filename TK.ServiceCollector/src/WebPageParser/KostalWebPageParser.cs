using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp;

namespace TK.WebPageParser
{
    public class KostalWebPageParser
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
            result.Add("PV.MeasureTime", DateTime.Now);
            string typeAndName = arrayParser.MoveToNextElementNotEmpty(0, true);
            string[] parseName = typeAndName.Split(new char[]
			{
				'\n'
			});
            string piko = string.Empty;
            string name = string.Empty;
            if (parseName.Length == 3)
            {
                piko = parseName[0].Trim();
                name = parseName[2].Trim();
            }
            result.Add("PV.PikoType", piko);
            result.Add("PV.Name", name);
            result.Add("PV.CurrentACPower", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("aktuell", 1, true)));
            result.Add("PV.ProducedEnergy", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Gesamtenergie", 1, true)));
            result.Add("PV.DailyEnergy", ArrayParser.ConvertToDoubleInvariantCulture(arrayParser.SearchForExactElement("Tagesenergie", 1, true)));
            result.Add("PV.Status", arrayParser.SearchForExactElement("Status", 1, true));
            result.Add("PV.String1Voltage", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Spannung", 1, true)));
            result.Add("PV.L1Voltage", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Spannung", 1, true)));
            result.Add("PV.String1Current", ArrayParser.ConvertToDoubleInvariantCulture(arrayParser.SearchForExactElement("Strom", 1, true)));
            result.Add("PV.L1Power", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Leistung", 1, true)));
            result.Add("PV.String2Voltage", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Spannung", 1, true)));
            result.Add("PV.L2Voltage", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Spannung", 1, true)));
            result.Add("PV.String2Current", ArrayParser.ConvertToDoubleInvariantCulture(arrayParser.SearchForExactElement("Strom", 1, true)));
            result.Add("PV.L2Power", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Leistung", 1, true)));
            result.Add("PV.L3Voltage", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Spannung", 1, true)));
            result.Add("PV.L3Power", ArrayParser.ConvertToInt(arrayParser.SearchForExactElement("Leistung", 1, true)));
            return result;
        }
    }
}