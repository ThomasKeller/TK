namespace TK.TimeSeries.Core
{
    public enum OPCQuality
    {
        NoValue = 255,           // OPC No Quality/No Value
        CompressedValue = 254,   // My indicatior for an ignored value

        Bad = 0,                 // OPC Quality Bad/Non Specific
        ConfigurationError = 4,  // OPC Quality Bad/Configuration Error
        NotConnected = 8,        // OPC Quality Bad/Not Connected
        DeviceFailute = 12,      // OPC Quality Bad/Device Failure
        SensorFailure = 16,      // OPC Quality Bad/Sensor Failure
        LastKnownValue = 20,     // OPC Quality Bad/Last Known Value
        CommFailure = 24,        // OPC Quality Bad/Comm Failure
        OutOfService = 28,       // OPC Quality Bad/Out of Service

        Uncertain = 64,          // OPC Quality Uncertain/Non Specific
        LastUsableValue = 68,    // OPC Quality Uncertain/Last Usable Value
        SensorNotAccurate = 80,  // OPC Quality Uncertain/Sensor Not Accurate
        EUUnitsExceeded = 84,    // OPC Quality Uncertain/EU Units Exceeded
        SubNormal = 88,          // OPC Quality Uncertain/Sub Normal

        Good = 192,              // OPC Quality Good/Non Specific
        LocalOverride = 216,     // OPC Quality Good/Local Override
    }
}