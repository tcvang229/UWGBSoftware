#region Using Statements

using M.OBD._2;
using SQLite;

#endregion

namespace M.OBD2
{
    public class UserSetting
    {
        // Db
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        // If user selected calculation/units is imperial or metric: default as imperial
        public bool isMetric { get; set; }

        // Bluetooth device name
        [MaxLength(100)]
        public string Device_Name { get; set; }

        // Bluetooth device address
        [MaxLength(100)]
        public string Device_Address { get; set; }

        // If logging starts automatically
        public bool isLoggingAuto { get; set; }

        // Log file path
        [MaxLength(100)]
        public string LoggingPath { get; set; }

        // Test Mode
        public bool isTestMode { get; set; }

        // Non Db
        [Ignore]
        public UserSettings.UNIT_TYPE Unit_Type { get; set; }

        [Ignore]
        public string DEFAULT_LOG_PATH { get; set; }
    }

    public class UserSettings
    {
        private readonly UserSetting oUserSetting;

        public enum UNIT_TYPE
        {
            NONE,
            IMPERIAL,
            METRIC
        }

        public UserSettings()
        {
            oUserSetting = new UserSetting();

            // ToDo: Table insert completes successfully however does not load on restart
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {
                try
                {
                    int rows = connection.Table<UserSetting>().Count();
                    System.Diagnostics.Debug.WriteLine("----ROWS----");
                }
                catch (SQLiteException e)
                {
                    System.Diagnostics.Debug.WriteLine("----ROWS EXCEPTION----");
                    UserSettingInitializer.Initialize();
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // ToDo: This line is a temporary fix for db failed table creation - needs to be removed ///////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            oUserSetting = UserSettingInitializer.GetUserSetting();

            UpdateUserUnits();
        }

        public void SetIsTestMode(bool value)
        {
            oUserSetting.isTestMode = value;
        }

        public bool GetIsTestMode()
        {
            return oUserSetting.isTestMode;
        }

        public bool GetIsMetric()
        {
            return oUserSetting.isMetric;
        }

        public void SetIsMetric(bool value)
        {
            oUserSetting.isMetric = value;
        }

        public bool isUserDevice()
        {
            return !string.IsNullOrEmpty(oUserSetting.Device_Name) && !string.IsNullOrEmpty(oUserSetting.Device_Address);
        }

        public bool GetMetricUnits()
        {
            return oUserSetting.isMetric;
        }

        public bool GetImperialUnits()
        {
            return oUserSetting.isMetric;
        }

        public UserSettings.UNIT_TYPE GetUserUnits()
        {
            return oUserSetting.Unit_Type;
        }

        public string GetDeviceAddress()
        {
            return oUserSetting.Device_Address;
        }

        public string GetDeviceName()
        {
            return oUserSetting.Device_Name;
        }

        public void UpdateUserUnits()
        {
            oUserSetting.Unit_Type = (oUserSetting.isMetric) ? UNIT_TYPE.METRIC : UNIT_TYPE.IMPERIAL;
        }

        public bool GetLoggingAuto()
        {
            return oUserSetting.isLoggingAuto;
        }
    }
}
