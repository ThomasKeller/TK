using System.Collections.Generic;
using System.Globalization;

namespace TK.WebPageParser
{
    public class ArrayParser
    {
        private enum ArrayParserSearchCriterion
        {
            Exact,
            StartWith,
            EndsWith,
            PartOf,
            MoveToNextElementNotEmpty
        }

        private string[] _arrayToParse;
        private int _currentPosition;

        public ArrayParser(string[] array)
        {
            List<string> trimmedArray = new List<string>();
            for (int i = 0; i < array.Length; i++)
            {
                string item = array[i];
                trimmedArray.Add(item.Trim());
            }
            this._arrayToParse = trimmedArray.ToArray();
            this._currentPosition = 0;
        }

        private ArrayParser(IEnumerable<string> elements)
        {
            List<string> array = new List<string>();
            foreach (string item in elements)
            {
                array.Add(item.Trim());
            }
        }

        public void MoveNext()
        {
            this._currentPosition++;
        }

        public string MoveToNextElementNotEmpty(int resultOffset, bool moveCurrentPositionToOffset)
        {
            return this.SearchFor(string.Empty, resultOffset, ArrayParser.ArrayParserSearchCriterion.MoveToNextElementNotEmpty, moveCurrentPositionToOffset);
        }

        public string SearchForExactElement(string searchText, int resultOffset, bool moveCurrentPositionToOffset)
        {
            return this.SearchFor(searchText, resultOffset, ArrayParser.ArrayParserSearchCriterion.Exact, moveCurrentPositionToOffset);
        }

        private string SearchFor(string searchText, int resultOffset, ArrayParser.ArrayParserSearchCriterion criterion, bool moveCurrentPositionToOffset)
        {
            bool found = false;
            string result = null;
            if (this._currentPosition >= this._arrayToParse.Length || this._currentPosition < 0)
            {
                return null;
            }
            if (criterion == ArrayParser.ArrayParserSearchCriterion.Exact)
            {
                while (this._currentPosition < this._arrayToParse.Length)
                {
                    if (this._arrayToParse[this._currentPosition] == searchText)
                    {
                        found = true;
                        break;
                    }
                    this._currentPosition++;
                }
            }
            if (criterion == ArrayParser.ArrayParserSearchCriterion.MoveToNextElementNotEmpty)
            {
                while (this._currentPosition < this._arrayToParse.Length)
                {
                    if (this._arrayToParse[this._currentPosition].Length > 0)
                    {
                        found = true;
                        break;
                    }
                    this._currentPosition++;
                }
            }
            if (found)
            {
                result = this._arrayToParse[this._currentPosition + resultOffset];
                if (moveCurrentPositionToOffset)
                {
                    this._currentPosition += resultOffset;
                }
            }
            return result;
        }

        public static object ConvertToInt(string value)
        {
            int i = 0;
            if (int.TryParse(value, out i))
            {
                return i;
            }
            return null;
        }

        public static object ConvertToDoubleInvariantCulture(string value)
        {
            double d = 0.0;
            if (double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
            {
                return d;
            }
            return null;
        }

        public static object ConvertToDouble(string value, CultureInfo cultureInfo)
        {
            double d = 0.0;
            if (double.TryParse(value, NumberStyles.AllowDecimalPoint, cultureInfo, out d))
            {
                return d;
            }
            return null;
        }
    }
}