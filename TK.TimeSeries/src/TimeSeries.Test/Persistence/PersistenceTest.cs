namespace TK.TimeSeries.Test.Core
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using TK.TimeSeries.Core;
    using TK.TimeSeries.Persistence;

    [TestFixture]
    public class PersistenceTest
    {
        [SetUp]
        public void SetUp()
        {
            DataBaseConnection.InitSqlCeConnection("Data Source=MeasuredValueDB.sdf;Password=;Persist Security Info=True");
            DataBaseConnection.InitSqlConnection("Data Source=.;Initial Catalog=HomeAutomation;Integrated Security=True");
        }

        [Test, Description("GetConnectionCheck")]
        public void GetConnectionCheck()
        {
            var now = DateTime.Now;
            var mv = new MeasuredValue() {
                Name = "",
                Description = "meine beschreibung",
                TimeStamp = now,
                Value = "test",
                Quality = OPCQuality.NoValue
            };

            int affectedRows = ValueTableWriter.SaveValueWhenConditionsAreMet(mv, CompressionCondition.GetDefaultCondition());
            Assert.AreEqual(0, affectedRows);
            //ValueTableWriter valueTable = new ValueTableWriter();
            //MeasuredValue mv = valueTable.ReadLastMeasuredValueFromLocalDB("test", TypeCode.Double);
            //Assert.AreEqual(OPCQuality.NoValue, mv.Quality);
        }

        [Test, Description("InsertIntoDoubleTable")]
        public void InsertIntoDoubleTable()
        {
            DateTime now = DateTime.Now;
            string tagName = "MyMeasuredValue";
            double tagValue = 1.2345678;
            string remark = "Bla Bla";
            int count = ValueTableWriter.SaveValueWhenConditionsAreMet(
                new MeasuredValue() {
                    Name = tagName,
                    TimeStamp = now,
                    Quality = OPCQuality.Good,
                    Value = tagValue,
                    Description = remark
                }, CompressionCondition.GetNoCompressionCondition());
            Assert.AreEqual(1, count);

            count = ValueTableWriter.SaveValueWhenConditionsAreMet(
                new MeasuredValue() {
                    Name = tagName,
                    TimeStamp = now + new TimeSpan(0, 0, 0, 1, 1),
                    Quality = OPCQuality.Good,
                    Value = tagValue,
                    Description = remark
                }, CompressionCondition.GetDefaultCondition());

            MeasuredValue mv = ValueTableWriter.ReadLastMeasuredValueFromLocalDB("MyMeasuredValue", TypeCode.Double);
            Assert.AreEqual(tagName, mv.Name);
            TimeSpan sp = now - mv.TimeStamp;
            // Datenbankgenauigkeit ist bei Datum nicht hoch genug
            Assert.Greater(0.1, sp.TotalSeconds);
            Assert.AreEqual(tagValue, mv.Value);
            Assert.AreEqual(OPCQuality.Good, mv.Quality);
            Assert.AreEqual(remark, mv.Description);
        }

        [Test, Description("InsertIntoIntTable")]
        public void InsertIntoIntTable()
        {
            DateTime now = DateTime.Now;
            string tagName = "MyMeasuredValue";
            int tagValue = 12345678;
            string remark = "Bla Bla";
            int count = ValueTableWriter.SaveValueWhenConditionsAreMet(
                new MeasuredValue() {
                    Name = tagName,
                    TimeStamp = now,
                    Quality = OPCQuality.Good,
                    Value = tagValue,
                    Description = remark
                }, CompressionCondition.GetNoCompressionCondition());
            Assert.AreEqual(1, count);

            count = ValueTableWriter.SaveValueWhenConditionsAreMet(
                new MeasuredValue() {
                    Name = tagName,
                    TimeStamp = now + new TimeSpan(0, 0, 0, 1, 1),
                    Quality = OPCQuality.Good,
                    Value = tagValue,
                    Description = remark
                }, CompressionCondition.GetDefaultCondition());
            Assert.AreEqual(0, count);

            MeasuredValue mv = ValueTableWriter.ReadLastMeasuredValueFromLocalDB("MyMeasuredValue", TypeCode.Int32);
            Assert.AreEqual(tagName, mv.Name);
            TimeSpan sp = now - mv.TimeStamp;
            // Datenbankgenauigkeit ist bei Datum nicht hoch genug
            Assert.Greater(0.1, sp.TotalSeconds);
            Assert.AreEqual(tagValue, mv.Value);
            Assert.AreEqual(OPCQuality.Good, mv.Quality);
            Assert.AreEqual(remark, mv.Description);
        }

        [Test, Description("InsertIntoBoolTable")]
        public void InsertIntoBoolTable()
        {
            DateTime now = DateTime.Now;
            string tagName = "MyMeasuredValue";
            bool tagValue = true;
            string remark = "Bla Bla";
            int count = ValueTableWriter.SaveValueWhenConditionsAreMet(
                new MeasuredValue() {
                    Name = tagName,
                    TimeStamp = now,
                    Quality = OPCQuality.Good,
                    Value = tagValue,
                    Description = remark
                }, CompressionCondition.GetNoCompressionCondition());
            Assert.AreEqual(1, count);

            count = ValueTableWriter.SaveValueWhenConditionsAreMet(
                new MeasuredValue() {
                    Name = tagName,
                    TimeStamp = now + new TimeSpan(0, 0, 0, 1, 1),
                    Quality = OPCQuality.Good,
                    Value = tagValue,
                    Description = remark
                }, CompressionCondition.GetDefaultCondition());
            Assert.AreEqual(0, count);

            MeasuredValue mv = ValueTableWriter.ReadLastMeasuredValueFromLocalDB("MyMeasuredValue", TypeCode.Boolean);
            Assert.AreEqual(tagName, mv.Name);
            TimeSpan sp = now - mv.TimeStamp;
            // Datenbankgenauigkeit ist bei Datum nicht hoch genug
            Assert.Greater(0.1, sp.TotalSeconds);
            Assert.AreEqual(tagValue, mv.Value);
            Assert.AreEqual(OPCQuality.Good, mv.Quality);
            Assert.AreEqual(remark, mv.Description);
        }

        [Test, Description("InsertIntoStringTable")]
        public void InsertIntoStringTable()
        {
            DateTime now = DateTime.Now;
            string tagName = "MyMeasuredValue";
            string tagValue = "Thomas";
            string remark = "Bla Bla";
            int count = ValueTableWriter.SaveValueWhenConditionsAreMet(
                new MeasuredValue() {
                    Name = tagName,
                    TimeStamp = now,
                    Quality = OPCQuality.Good,
                    Value = tagValue,
                    Description = remark
                }, CompressionCondition.GetNoCompressionCondition());
            Assert.AreEqual(1, count);

            count = ValueTableWriter.SaveValueWhenConditionsAreMet(
                new MeasuredValue() {
                    Name = tagName,
                    TimeStamp = now + new TimeSpan(0, 0, 0, 1, 1),
                    Quality = OPCQuality.Good,
                    Value = tagValue,
                    Description = remark
                }, CompressionCondition.GetDefaultCondition());
            Assert.AreEqual(0, count);

            MeasuredValue mv = ValueTableWriter.ReadLastMeasuredValueFromLocalDB("MyMeasuredValue", TypeCode.String);
            Assert.AreEqual(tagName, mv.Name);
            TimeSpan sp = now - mv.TimeStamp;
            // Datenbankgenauigkeit ist bei Datum nicht hoch genug
            Assert.Greater(0.1, sp.TotalSeconds);
            Assert.AreEqual(tagValue, mv.Value);
            Assert.AreEqual(OPCQuality.Good, mv.Quality);
            Assert.AreEqual(remark, mv.Description);
        }

        /*[Test, Description("GetAvailableTagNamesFor")]
        [Ignore("check by the code contruct")]
        public void GetAvailableTagNamesFor()
        {
            IDictionary<string, DateTime> result = null;

            ValueTableWriter.ReadLastMeasuredValueFromLocalDB(null, TypeCode.Int32);

            ValueTableWriter.TransferDataToDestDB();
        }*/
    }
}