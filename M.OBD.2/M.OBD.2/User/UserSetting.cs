namespace M.OBD2
{
    public class UserSetting
    {
        // Db
        // If user selected calculation/units is imperial or metric: default as imperial
        private readonly bool isMetric;

        // Bluetooth device name
        private readonly string Device_Name;

        // Bluetooth device address
        private readonly string Device_Address;

        // If logging is user enabled/disabled
        private bool isLoggingEnabled;

        // If logging starts automatically
        private bool isLoggingAuto;

        // Log file path
        private string LoggingPath;

        // Non Db
        private UNIT_TYPE Unit_Type;
        private string DEFAULT_LOG_PATH = ""; // ToDo: add a default file path

        public enum UNIT_TYPE
        {
            IMPERIAL,
            METRIC
        }

        public UserSetting() // ToDo: Populate from Db
        {
            // Db loading here


            // ToDo: Remove hardcoded values after db is implemented
            isMetric = false;
            Device_Name = "OBDII";
            Device_Address = "00:1D:A5:05:4F:05";
            isLoggingEnabled = true;
            isLoggingAuto = false;

            // ToDo: Make OS path fixed or add user option ex. internal/external storage?
            LoggingPath = (string.IsNullOrEmpty(LoggingPath)) ? DEFAULT_LOG_PATH : LoggingPath;

            UpdateUserUnits();
        }

        public string GetDeviceAddress()
        {
            return Device_Address;
        }

        public string GetDeviceName()
        {
            return Device_Name;
        }

        public bool isUserDevice()
        {
            return !string.IsNullOrEmpty(Device_Name) && !string.IsNullOrEmpty(Device_Address);
        }

        public bool GetMetricUnits()
        {
            return isMetric;
        }

        public bool GetImperialUnits()
        {
            return isMetric;
        }

        public UNIT_TYPE GetUserUnits()
        {
            return Unit_Type;
        }

        public void UpdateUserUnits()
        {
            Unit_Type = (isMetric) ? UNIT_TYPE.METRIC : UNIT_TYPE.IMPERIAL;
        }

        public bool GetLoggingEnabled()
        {
            return isLoggingEnabled;
        }

        public void SetLoggingEnabled(bool value)
        {
            isLoggingEnabled = value;
        }

        public bool GetLoggingAuto()
        {
            return isLoggingAuto;
        }

        public void SetLoggingAuto(bool value)
        {
            isLoggingAuto = value;
        }

        public string GetLoggingPath()
        {
            return LoggingPath;
        }

        public void SetLoggingPath(string value)
        {
            if (!string.IsNullOrEmpty(value))
                LoggingPath = value;
        }
    }
}
