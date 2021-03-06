﻿// -----------------------------------------------------------------------
// <copyright file="CompressionConditionConfigSerializerTest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TK.TimeSeries.Test.Persistence
{
    using System.IO;
    using System.Xml.Serialization;
    using NUnit.Framework;
    using TK.TimeSeries.Core;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class CompressionConditionConfigSerializerTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public void SaveCompressionConditionConfig()
        {
            TimeSpanConfig tsc1 = new TimeSpanConfig() {
                Hours = 1,
                Minutes = 2,
                Seconds = 3
            };

            TimeSpanConfig tsc2 = new TimeSpanConfig() {
                Hours = 0,
                Minutes = 1,
                Seconds = 2
            };

            CompressionConditionConfig ccc = new CompressionConditionConfig() {
                MeasuredValueName = "Test",
                RewriteValueAfter = tsc1,
                TimeDeadBand = tsc2,
                ValueDeadBandDelta = 0.12345
            };

            CompressionConditionConfigs configs = new CompressionConditionConfigs();

            configs.Items = new CompressionConditionConfig[] {
                ccc,
                ccc
            };

            XmlSerializer configSerializer = new XmlSerializer(typeof(CompressionConditionConfigs), "TimeSeries");
            using (TextWriter writer = new StreamWriter("test.xml")) {
                configSerializer.Serialize(writer, configs);
                writer.Close();
            }

            XmlSerializer test = new XmlSerializer(typeof(CompressionConditionConfigs), "TimeSeries");
            using (TextReader reader = new StreamReader("test.xml")) {
                CompressionConditionConfigs configs2 = (CompressionConditionConfigs)test.Deserialize(reader);

                Assert.AreEqual(2, configs2.Items.Length);

                CompressionConditionManager manager = new CompressionConditionManager(configs2);

                var config = manager.GetConfigFor("");
            }


            var configs10 = new CompressionConditionConfigs();

            configs10.Items = new CompressionConditionConfig[] { 
                new CompressionConditionConfig() {
                    MeasuredValueName = "Tag1", 
                    RewriteValueAfter = new TimeSpanConfig() {
                        Hours = 1,
                        Minutes = 0,
                        Seconds = 0 
                    },
                    TimeDeadBand = new TimeSpanConfig () {
                        Hours = 0,
                        Minutes = 0,
                        Seconds = 10 
                    },
                    ValueDeadBandDelta = 1.2
                },
                new CompressionConditionConfig() {
                    MeasuredValueName = "Tag2", 
                    RewriteValueAfter = new TimeSpanConfig() {
                        Hours = 1,
                        Minutes = 0,
                        Seconds = 0 
                    },
                    TimeDeadBand = new TimeSpanConfig () {
                        Hours = 0,
                        Minutes = 0,
                        Seconds = 10 
                    },
                    ValueDeadBandDelta = 1.2
                }
            };

            //XmlSerializer configSerializer = new XmlSerializer(typeof(CompressionConditionConfigs), "TimeSeries");
            using (TextWriter writer = new StreamWriter("Thomas.xml"))
            {
                configSerializer.Serialize(writer, configs10);
                writer.Close();
            }




            CompressionConditionManager manager2 = CompressionConditionManager.LoadFromFile("Thomas.xml");
        }
    }
}