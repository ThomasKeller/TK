using System;
using TK.Logging;
using System.Text;

namespace TK.TimeSeries.Core
{
    /// <summary>
    /// Define the criterias for storing the measured values
    ///   TimeDeadBand:  Define how much time must be gone
    ///                  before writing the next value.
    ///   ValueDeadBand: Define by how many percent the measured value must have changed
    ///                  before a new value is written.
    ///   RewriteAfter:  Define that after this time a new value is written
    ///                  with the current timestammp.
    /// </summary>
    public class CompressionCondition
    {
        #region Fields

        private static ILogger _logger = LoggerFactory.CreateLoggerFor(typeof(CompressionCondition));
        private readonly static CompressionCondition _default = new CompressionCondition();
        private readonly static CompressionCondition _noCompression;
        private readonly StringBuilder _sb = new StringBuilder();
        private double _valueDeadBandDelta = 0;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Define how much time must be gone before writing the next value.
        /// Zero is interpreted as no deadband
        /// </summary>
        public TimeSpan TimeDeadBand { get; set; }
        /// <summary>
        /// Define by how many delta the measured value must have changed
        /// before a new value is written.
        /// Zero is interpreted as no deadband
        /// </summary>
        public double ValueDeadBandDelta
        {
            get { return _valueDeadBandDelta; }
            set { _valueDeadBandDelta = value < 0 ? -value : value; }
        }
        /// <summary>
        /// Define that after this time a new value is written
        /// with the current timestammp.
        /// </summary>
        public TimeSpan RewriteValueAfter { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Static Constructor
        /// </summary>
        static CompressionCondition()
        {
            _noCompression = new CompressionCondition() {
                RewriteValueAfter = new TimeSpan(),
                ValueDeadBandDelta = 0,
                TimeDeadBand = new TimeSpan()
            };
        }

        /// <summary>
        /// Standard Constructor
        /// Set the standard conditions
        /// TimeDeadBand = 1 s
        /// RewriteValueAfter = 4 hours
        /// ValueDeadBand = 0.00001%
        /// </summary>
        public CompressionCondition()
        {
            TimeDeadBand = new TimeSpan(0, 0, 1);
            RewriteValueAfter = new TimeSpan(4, 0, 0);
            ValueDeadBandDelta = 0.0001;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Returns an instance with the default conditons
        /// This is an static instance
        /// </summary>
        public static CompressionCondition GetDefaultCondition()
        {
            return _default;
        }

        /// <summary>
        /// Returns an instance with the "no compression" conditons
        /// This is an static instance
        /// </summary>
        public static CompressionCondition GetNoCompressionCondition()
        {
            return _noCompression;
        }

        /// <summary>
        /// Check all conditions and decide if the current should be written
        /// </summary>
        /// <param name="currentValue">the current measured value</param>
        /// <param name="previousValue">the previous measured value </param>
        /// <returns></returns>
        public bool ShouldCurrentValueBeWritten(MeasuredValue currentValue, MeasuredValue previousValue)
        {
            if (currentValue == null) { throw new ArgumentNullException("currentValue"); }
            if (previousValue == null) { throw new ArgumentNullException("previousValue"); }

            _logger.DebugFormat("Previous value: {0}", previousValue.ToString());
            _logger.DebugFormat("Current value:  {0}", currentValue.ToString());

            // OPCQuality.NoValue means that no value is available
            // OPCQuality.NoValue should never be written to the database
            // if current value is "NoValue" => don't write value
            if (currentValue.Quality == OPCQuality.NoValue) {
                _logger.Debug("don't write value: OPCQuality = NoValue");
                return false;
            }

            // if the quality of the previous value was "NoValue"
            // then write the current, because this is the first value ever
            if (previousValue.Quality == OPCQuality.NoValue) {
                _logger.InfoFormat("write value (no previous value): {0}", currentValue.ToString());
                return true;
            }

            // both must be true to write value
            if (CheckConditionTimeDeadBandIsMet(currentValue, previousValue)) {
                if (CheckConditionValueDeadBandDeltaIsMet(currentValue, previousValue)) {
                    _logger.InfoFormat("write value: Current: {0} / Previous {1}", currentValue.ToString(), previousValue.ToString());
                    return true;
                }
            }

            // if all the conditions above are not meet, check if it is time to write a value
            // but then set the current time to the current value
            if (CheckConditionRewriteValueAfterIsMet(currentValue, previousValue)) {
                currentValue.TimeStamp = DateTime.Now;
                _logger.InfoFormat("write value (rewrite after): Current: {0} / Previous {1}", currentValue.ToString(), previousValue.ToString());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the condition "RewriteValueAfter" is met
        /// Condition: The previous value was written earlier than
        /// "now" - RewriteValue
        /// </summary>
        /// <param name="currentValue">the current measured value</param>
        /// <param name="previousValue">the previous measured value </param>
        /// <returns></returns>
        public bool CheckConditionRewriteValueAfterIsMet(MeasuredValue currentValue, MeasuredValue previousValue)
        {
            if (currentValue == null) { throw new ArgumentNullException("currentValue"); }
            if (previousValue == null) { throw new ArgumentNullException("previousValue"); }

            TimeSpan sp = currentValue.TimeStamp - previousValue.TimeStamp;
            if (RewriteValueAfter.TotalSeconds < sp.TotalSeconds) {
                _logger.DebugFormat("RewriteValueAfterIsMet: {0} < {1}", RewriteValueAfter.TotalSeconds, sp.TotalSeconds);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the condition "TimeDeadBand" is met
        /// Condition: The previous value was written before
        /// "now" - TimeDeadBand
        /// </summary>
        /// <param name="currentValue">the current measured value</param>
        /// <param name="previousValue">the previous measured value </param>
        /// <returns></returns>
        public bool CheckConditionTimeDeadBandIsMet(MeasuredValue currentValue, MeasuredValue previousValue)
        {
            if (currentValue == null) { throw new ArgumentNullException("currentValue"); }
            if (previousValue == null) { throw new ArgumentNullException("previousValue"); }
            TimeSpan sp = currentValue.TimeStamp - previousValue.TimeStamp;
            if (sp.TotalSeconds > TimeDeadBand.TotalSeconds) {
                _logger.DebugFormat("TimeDeadBandIsMet: {0} > {1}", sp.TotalSeconds, TimeDeadBand.TotalSeconds);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the condition "ValueDeadBandDelta" is met
        /// Condition: The absolute difference between
        /// the currrent and the previous value is bigger than
        /// ValueDeadBandDelta
        /// </summary>
        /// <param name="currentValue">the current measured value</param>
        /// <param name="previousValue">the previous measured value </param>
        /// <returns></returns>
        public bool CheckConditionValueDeadBandDeltaIsMet(MeasuredValue currentValue, MeasuredValue previousValue)
        {
            if (currentValue == null) { throw new ArgumentNullException("currentValue"); }
            if (previousValue == null) { throw new ArgumentNullException("previousValue"); }
            if (ValueDeadBandDelta == 0) {
                _logger.DebugFormat("ValueDeadBandPercentIsMet: ValueDeadBandPercent = {0}", ValueDeadBandDelta);
                return true;
            }
            bool equal = currentValue.Value.ToString() == previousValue.Value.ToString();
            if (equal)
                return false;
            double result = 0;
            double value1 = 0;
            double value2 = 0;
            switch (currentValue.GetTypeCode()) {
                case TypeCode.Double:
                    value1 = (double)previousValue.Value;
                    value2 = (double)currentValue.Value;
                    break;
                case TypeCode.Int32:
                    value1 = Convert.ToDouble((int)previousValue.Value);
                    value2 = Convert.ToDouble((int)currentValue.Value);
                    break;
                default: 
                    return true;
            }
            if (value1 > value2) {
                result = value2;
                value2 = value1;
                value1 = result;
            }
            result = Math.Abs(value2 - value1);
            _logger.DebugFormat("ValueDeadBandDeltaIsMet: {0} Result: {1} %: {2}", result > ValueDeadBandDelta, result, ValueDeadBandDelta);
            return result > ValueDeadBandDelta;
        }

        public override string ToString()
        {
            return string.Format("TimeDeadBand: {0} ValueDeadBand: {1}%  RewriteValueAfter: {2}",
                this.TimeDeadBand,
                this.ValueDeadBandDelta,
                this.RewriteValueAfter);
        }

        #endregion Methods
    }
}