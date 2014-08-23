using System;
using System.IO;
using System.IO.Ports;
using TK.Logging;
using TK.PluginManager;
using TK.SimpleMessageQueue;

namespace EMH_eHZ_Plugin.SerialPort
{
    public class SerialPortLogger
    {
        private static ILogger _Logger = LoggerFactory.GetCurrentClassLogger();

        private const string c_QueuePath = @".\Private$\EMH_eHZ_Plugin";
        private const string c_ProducedEnergy1 = "ProducedEnergy1";
        private const string c_ProducedEnergy2 = "ProducedEnergy2";
        private const string c_ConsumedEnergy1 = "ConsumedEnergy1";
        private const string c_ConsumedEnergy2 = "ConsumedEnergy2";
        private const string c_ProducedPower = "ProducedPower";
        private const string c_ConsumedPower = "ConsumedPower";
        private const string c_Watt = "W";
        private const string c_KiloWattHour = "kWh";

        private MemoryStream _MemoryStream = new MemoryStream();
        private LastMeasurement<double> _LastComsumedEnergy_Wh = new LastMeasurement<double>();
        private LastMeasurement<double> _LastProducedEnergy_Wh = new LastMeasurement<double>();
        private System.IO.Ports.SerialPort _SerialPort;
        private DateTime? _TimeOfLastValidValue;
        private int _CalculatePowerAfter_Sec = 30;
        private int _ReadNumberOfResultBeforeWattCalculation = 0;

        private SimpleMessageQueueWrapper<MeasureValueBox> _Queue = new SimpleMessageQueueWrapper<MeasureValueBox>();

        public SerialPortLogger(string comPort,
                                int calculatePowerAfter_sec = 30)
        {
            _Queue.Initialize(c_QueuePath);
            _CalculatePowerAfter_Sec = calculatePowerAfter_sec;
            InitializeSerialPort(comPort);
        }

        ~SerialPortLogger()
        {
            _Logger.Info("Close SerialPort:" + _SerialPort.PortName);
            _SerialPort.Close();
            _SerialPort.Dispose();
        }

        private void InitializeSerialPort(string comPort)
        {
            _SerialPort = new System.IO.Ports.SerialPort(comPort);
            try
            {
                Directory.CreateDirectory("Data");
                _Logger.Info("Open SerialPort:" + comPort);
                _SerialPort.BaudRate = 9600;
                _SerialPort.Parity = Parity.None;
                _SerialPort.StopBits = StopBits.One;
                _SerialPort.DataBits = 8;
                _SerialPort.Handshake = Handshake.None;
                _SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                _SerialPort.Open();
                _SerialPort.RtsEnable = true;
                _SerialPort.DtrEnable = true;
                _Logger.Info("Port Status" + _SerialPort.IsOpen.ToString());
                _SerialPort.RtsEnable = true;
            }
            catch (Exception ex)
            {
                _Logger.ErrorFormat("Fatal Error: Close Serial Port: {0}", ex.Message);
                _SerialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                _SerialPort.Close();
                _SerialPort.Dispose();
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = (System.IO.Ports.SerialPort)sender;
            if (sp.BytesToRead < 392 * 2 - 1)
            {
                return;
            }
            try
            {
                IStreamReader stream = new SerialPortStreamReader(sp);
                SmlMessageFinder smlCutter = new SmlMessageFinder(stream);
                if (smlCutter.Found)
                {
                    ParseSmlMessage(smlCutter);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message);
                sp.BaseStream.Flush();
            }
        }

        private void ParseSmlMessage(SmlMessageFinder smlCutter)
        {
            SmlMessageParser parser = new SmlMessageParser(smlCutter.Message);
            if (parser.IsValid)
            {
                _Logger.DebugFormat("Consumed: {0} Produced: {1}", parser.ConsumedEnergy1_Wh, parser.ProducedEnergy1_Wh);
                if (_TimeOfLastValidValue.HasValue)
                {
                    TimeSpan lastReadDuration = DateTime.Now - _TimeOfLastValidValue.Value;
                    if (lastReadDuration.TotalMinutes > 1)
                    {
                        _Logger.InfoFormat("last valid read from serial port is {0} minutes old", lastReadDuration.TotalMinutes);
                        _ReadNumberOfResultBeforeWattCalculation = 5;
                    }
                }
                _TimeOfLastValidValue = DateTime.Now;
                SaveEnergyValues(smlCutter, parser);
                if (_ReadNumberOfResultBeforeWattCalculation <= 0)
                {
                    CalculateConsumedPower(smlCutter, parser);
                    CalculateProducedPower(smlCutter, parser);
                }
                else
                {
                    _ReadNumberOfResultBeforeWattCalculation--;
                }
            }
            else
            {
                string fileName = string.Format("Data\\{0:yyMMdd-HHmmss}.sml", DateTime.Now);
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Write(smlCutter.Message, 0, smlCutter.Message.Length);
                    fs.Close();
                }
            }
        }

        private void SaveEnergyValues(SmlMessageFinder smlCutter, SmlMessageParser parser)
        {
            var box = new MeasureValueBox();
            box.MeasuredUtcTime = DateTime.UtcNow;
            box.MeasuredValues.Add("EHZ.MeasureTime", DateTime.Now);
            box.MeasuredValues.Add("EHZ.ConsumedEnergy1", parser.ConsumedEnergy1_Wh);
            box.MeasuredValues.Add("EHZ.ConsumedEnergy2", parser.ConsumedEnergy2_Wh);
            box.MeasuredValues.Add("EHZ.ProducedEnergy1", parser.ProducedEnergy1_Wh);
            box.MeasuredValues.Add("EHZ.ProducedEnergy2", parser.ProducedEnergy2_Wh);
            box.MeasuredValues.Add("EHZ.Device", parser.Device);
            box.MeasuredValues.Add("EHZ.CompanyName", parser.CompanyName);
            _Queue.Send(box);
        }

        private void CalculateProducedPower(SmlMessageFinder smlCutter, SmlMessageParser parser)
        {
            if (false == _LastProducedEnergy_Wh.MeasuredTime.HasValue)
            {
                // first point
                _LastProducedEnergy_Wh.MeasuredTime = smlCutter.MeasuredTime;
                _LastProducedEnergy_Wh.Value = parser.ProducedEnergy1_Wh;
            }
            else
            {
                double? watt = CalcWattDifference(parser.ProducedEnergy1_Wh, smlCutter.MeasuredTime,
                                                  _LastProducedEnergy_Wh.Value, _LastProducedEnergy_Wh.MeasuredTime.Value);
                if (watt.HasValue)
                {
                    var box = new MeasureValueBox();
                    box.MeasuredUtcTime = _LastComsumedEnergy_Wh.MeasuredTime.Value.ToUniversalTime();
                    box.MeasuredValues.Add("EHZ.MeasureTime", _LastComsumedEnergy_Wh.MeasuredTime.Value);
                    box.MeasuredValues.Add("EHZ.ProducedPower", watt.Value);
                    _Queue.Send(box);
                    _LastProducedEnergy_Wh.MeasuredTime = smlCutter.MeasuredTime;
                    _LastProducedEnergy_Wh.Value = parser.ProducedEnergy1_Wh;
                }
            }
        }

        private void CalculateConsumedPower(SmlMessageFinder smlCutter, SmlMessageParser parser)
        {
            if (false == _LastComsumedEnergy_Wh.MeasuredTime.HasValue)
            {
                // first point
                _LastComsumedEnergy_Wh.MeasuredTime = smlCutter.MeasuredTime;
                _LastComsumedEnergy_Wh.Value = parser.ConsumedEnergy1_Wh;
            }
            else
            {
                double? watt = CalcWattDifference(parser.ConsumedEnergy1_Wh, smlCutter.MeasuredTime,
                                                  _LastComsumedEnergy_Wh.Value, _LastComsumedEnergy_Wh.MeasuredTime.Value);
                if (watt.HasValue)
                {
                    var box = new MeasureValueBox();
                    box.MeasuredUtcTime = _LastComsumedEnergy_Wh.MeasuredTime.Value.ToUniversalTime();
                    box.MeasuredValues.Add("EHZ.MeasureTime", _LastComsumedEnergy_Wh.MeasuredTime.Value);
                    box.MeasuredValues.Add("EHZ.ComsumedPower", watt.Value);
                    _Queue.Send(box);
                    _LastComsumedEnergy_Wh.MeasuredTime = smlCutter.MeasuredTime;
                    _LastComsumedEnergy_Wh.Value = parser.ConsumedEnergy1_Wh;
                }
            }
        }

        private double? CalcWattDifference(double energy_Wh, DateTime measuredTime, double preEnergy_Wh, DateTime prevMeasuredTime)
        {
            TimeSpan diff = DateTime.Now - prevMeasuredTime;
            TimeSpan diffHour = measuredTime - prevMeasuredTime;
            if (diffHour.TotalHours > 0 &&
                diff.TotalSeconds > _CalculatePowerAfter_Sec)
            {
                return (energy_Wh - preEnergy_Wh) / diffHour.TotalHours;
            }
            return null;
        }
    }
}