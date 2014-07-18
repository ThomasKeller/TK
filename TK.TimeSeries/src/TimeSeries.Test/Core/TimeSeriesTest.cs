// -----------------------------------------------------------------------
// <copyright file="TimeSeriesTest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TK.TimeSeries.Test.Core
{
    using System;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using NUnit.Framework;
    using TK.TimeSeries.Core;

    [TestFixture]
    public class TimeSeriesTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void WriteCompressionConditionConfig()
        {
            var value = new CompressionConditionConfig() {
                MeasuredValueName = "MyTag1",
                ValueDeadBandDelta = 1,
            };

            // RewriteValueAfter = new TimeSpan(1, 0, 0),
            // TimeDeadBand = new TimeSpan(0, 0, 1),
            value.SetTimeDeadBand(new TimeSpan(0, 0, 1));
            value.SetRewriteValueAfter(new TimeSpan(1, 0, 0));

            XmlSerializer x = new XmlSerializer(value.GetType());

            using (var writer = new XmlTextWriter("MyConfig.xml", UTF8Encoding.UTF8)) {
                x.Serialize(writer, value);
                //value.MeasuredValueName = "MyTag2";
                //x.Serialize(writer, value2);
            }
        }

        [Test, Description("Check Measured Value")]
        public void CheckMeasuredValue()
        {
            var mv = new MeasuredValue();
            mv.Name = "test";
            Assert.AreEqual(true, mv.IsValueNull());
            Assert.AreEqual(false, mv.IsValid());
            mv.Value = 1;
            Assert.AreEqual(false, mv.IsValueNull());
            Assert.AreEqual(false, mv.IsValid());
            Assert.AreEqual(TypeCode.Int32, mv.GetTypeCode());

            mv.TimeStamp = DateTime.Now;
            Assert.AreEqual(false, mv.IsValueNull());
            Assert.AreEqual(true, mv.IsValid());

            mv.Value = "text";
            Assert.AreEqual(TypeCode.String, mv.GetTypeCode());

            int value = 2;
            mv.Value = value;
            Assert.AreEqual("2", mv.Value.ToString());
            Assert.AreEqual(2, mv.Value);

            /*string test = JsonConvert.SerializeObject(mv);
            MeasuredValue mv2 = JsonConvert.DeserializeObject<MeasuredValue>(test);
            Assert.AreEqual(2, mv2.Value);

            mv.Value = "Test";
            test = JsonConvert.SerializeObject(mv);
            MeasuredValue mv3 = JsonConvert.DeserializeObject<MeasuredValue>(test);
            Assert.AreEqual("Test", mv3.Value);*/
        }

        [Test, Description("Check Exception for not implemented types")]
        [ExpectedException(typeof(Exception))]
        public void CheckMeasuredValueConverterException()
        {
            MeasuredValue mv = MeasuredValueConverter.ConvertTo("name", DateTime.Now, null, TypeCode.Empty);
        }

        [Test, Description("Check Measured Value Converter")]
        public void CheckMeasuredValueConverter()
        {
            string name = "TestName";
            string value = "Text";
            DateTime dt = DateTime.Now;

            MeasuredValue mv = MeasuredValueConverter.ConvertTo(name, dt, value, TypeCode.String);
            Assert.AreEqual(name, mv.Name);
            Assert.AreEqual(dt, mv.TimeStamp);
            Assert.AreEqual(value, mv.Value);
            Assert.AreEqual(TypeCode.String, mv.GetTypeCode());

            TypeCode tc = TypeCode.Int16;
            short shortValue = 100;
            value = shortValue.ToString();
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(name, mv.Name);
            Assert.AreEqual(dt, mv.TimeStamp);
            Assert.AreEqual(shortValue, mv.Value);
            Assert.AreEqual(tc, mv.GetTypeCode());
            Assert.AreEqual(OPCQuality.Good, mv.Quality);

            tc = TypeCode.Int32;
            int intValue = 110;
            value = intValue.ToString();
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(name, mv.Name);
            Assert.AreEqual(dt, mv.TimeStamp);
            Assert.AreEqual(intValue, mv.Value);
            Assert.AreEqual(tc, mv.GetTypeCode());
            Assert.AreEqual(OPCQuality.Good, mv.Quality);

            tc = TypeCode.Int64;
            long longValue = 120;
            value = longValue.ToString();
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(name, mv.Name);
            Assert.AreEqual(dt, mv.TimeStamp);
            Assert.AreEqual(longValue, mv.Value);
            Assert.AreEqual(tc, mv.GetTypeCode());
            Assert.AreEqual(OPCQuality.Good, mv.Quality);

            tc = TypeCode.Single;
            float floatValue = 1002.2f;
            value = floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(name, mv.Name);
            Assert.AreEqual(dt, mv.TimeStamp);
            Assert.AreEqual(floatValue, mv.Value);
            Assert.AreEqual(tc, mv.GetTypeCode());
            Assert.AreEqual(OPCQuality.Good, mv.Quality);

            tc = TypeCode.Double;
            double doubleValue = 2002.2;
            value = doubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(name, mv.Name);
            Assert.AreEqual(dt, mv.TimeStamp);
            Assert.AreEqual(doubleValue, mv.Value);
            Assert.AreEqual(tc, mv.GetTypeCode());
            Assert.AreEqual(OPCQuality.Good, mv.Quality);

            value = doubleValue.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("de"));
            MeasuredValueConverter.UsedCultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("de");
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(name, mv.Name);
            Assert.AreEqual(dt, mv.TimeStamp);
            Assert.AreEqual(doubleValue, mv.Value);
            Assert.AreEqual(tc, mv.GetTypeCode());
            Assert.AreEqual(OPCQuality.Good, mv.Quality);

            MeasuredValueConverter.UsedCultureInfo = System.Globalization.CultureInfo.InvariantCulture;
            tc = TypeCode.Boolean;
            bool boolValue = true;
            value = boolValue.ToString();
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(name, mv.Name);
            Assert.AreEqual(dt, mv.TimeStamp);
            Assert.AreEqual(boolValue, mv.Value);
            Assert.AreEqual(tc, mv.GetTypeCode());
            Assert.AreEqual(OPCQuality.Good, mv.Quality);

            value = "True";
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(boolValue, mv.Value);
            Assert.AreEqual(OPCQuality.Good, mv.Quality);

            value = "TRUE";
            mv = MeasuredValueConverter.ConvertTo(name, dt, value, tc);
            Assert.AreEqual(boolValue, mv.Value);
            Assert.AreEqual(OPCQuality.Good, mv.Quality);
        }
    }
}