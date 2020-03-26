using System;
using System.Collections.Generic;
using System.Text;

namespace M.OBD2
{
    public class UserSetting
    {
        // Db
        // If user selected calculation/units is imperial or metric: default as imperial
        private bool isMetric;

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

            UpdateUserUnits();
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
