using System;
using System.Collections.Generic;
using System.Text;

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

        // Non Db
        private UNIT_TYPE Unit_Type;

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

        public UNIT_TYPE GetUserUnits()
        {
            return Unit_Type;
        }

        public void UpdateUserUnits()
        {
            Unit_Type = (isMetric) ? UNIT_TYPE.METRIC : UNIT_TYPE.IMPERIAL;
        }
    }
}
