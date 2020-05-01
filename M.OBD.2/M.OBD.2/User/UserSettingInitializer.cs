using M.OBD._2;
using System;
using System.Collections.Generic;
using SQLite;

namespace M.OBD2
{
    public static  class UserSettingInitializer
    {
        public static void Initialize()
        {
            System.Diagnostics.Debug.WriteLine("---Creating Table---");
            CreateTable();
            System.Diagnostics.Debug.WriteLine("---Done---");
        }

        public static void CreateTable()
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {
                connection.CreateTable<UserSetting>();

                UserSetting oUserSetting = new UserSetting
                {
                    Id = 1,
                    isMetric = false,
                    Device_Name = string.Empty,
                    Device_Address = string.Empty,
                    isLoggingAuto = false,
                    LoggingPath = string.Empty,
                    isTestMode = true
                };

                connection.Insert(oUserSetting);
            }
        }
    }
}
