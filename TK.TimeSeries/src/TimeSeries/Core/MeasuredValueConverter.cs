using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace TK.TimeSeries.Core
{
    public class MeasuredValueConverter
    {
        public static CultureInfo UsedCultureInfo { get; set; }
        public static NumberStyles UsedNumberStyle { get; set; }

        static MeasuredValueConverter()
        {
            UsedCultureInfo = CultureInfo.InvariantCulture;
            UsedNumberStyle = NumberStyles.Any;
        }
        
        public static MeasuredValue ConvertTo(string name, DateTime timeStamp, string valueAsText, TypeCode targetType)
        {
            MeasuredValue mv = new MeasuredValue();
            bool convertState = false;
            mv.Name = name;
            mv.TimeStamp = timeStamp;
            switch (targetType) {
                case TypeCode.Boolean:
                    bool boolvalue = false;
                    convertState = bool.TryParse(valueAsText, out boolvalue);
                    mv.Value = boolvalue;
                    break;
                case TypeCode.Int16:
                    Int16 i16value = 0;
                    convertState = Int16.TryParse(valueAsText, out i16value);
                    mv.Value = i16value;
                    break;
                case TypeCode.Int32:
                    Int32 i32value = 0;
                    convertState = Int32.TryParse(valueAsText, out i32value);
                    mv.Value = i32value;
                    break;
                case TypeCode.Int64:
                    Int64 i64value = 0;
                    convertState = Int64.TryParse(valueAsText, UsedNumberStyle, UsedCultureInfo, out i64value);
                    mv.Value = i64value;
                    break;
                case TypeCode.Single:
                    float fvalue = 0;
                    convertState = float.TryParse(valueAsText, UsedNumberStyle, UsedCultureInfo, out fvalue);
                    mv.Value = fvalue;
                    break;
                case TypeCode.Double:
                    double dvalue = 0;
                    convertState = double.TryParse(valueAsText, UsedNumberStyle, UsedCultureInfo, out dvalue);
                    mv.Value = dvalue;
                    break;
                case TypeCode.String:
                    convertState = string.IsNullOrEmpty(valueAsText) == false;
                    mv.Value = valueAsText;
                    break;
                default:
                    throw new Exception("type currently not supported: " + targetType.ToString());
            }
            if (false == convertState) {
                mv.Quality = OPCQuality.NoValue;
            }
            return mv;
        }
    }
}
