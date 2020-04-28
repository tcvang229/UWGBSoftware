using M.OBD._2;
using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace M.OBD2
{
    public static class BluetoothCommandsInitializer
    {
        public static void InitializeTable()
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {
                /*BluetoothCmd ign = new BluetoothCmd()
                {
                    Id = 0,
                    Name = "IGN",
                    Units_Imperial = "",
                    Units_Metric = "",
                    Cmd = "ATIGN",
                    Rate = 1000,
                    Expression_Imperial = "",
                    Units_Imperial = "",
                    isRxBytes = false,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.DEFAULT }
                };*/

                BluetoothCmd coolant_temp = new BluetoothCmd()
                {
                    Id = 1,
                    Name = "coolant temp",
                    Expression_Imperial = "((a-40)*9/5+32)",
                    Units_Imperial = "F",
                    Expression_Metric = "(a - 40)",
                    Units_Metric = "C",
                    Cmd = "01051",
                    Rate = 3000,
                    Decimals = 0,
                    Value_Min = -40,
                    Value_Max = 215,
                    isRxBytes = true,
                    Bytes = 4,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.DEFAULT }
                };

                BluetoothCmd vss = new BluetoothCmd()
                {
                    Id = 2,
                    Name = "VSS",
                    Expression_Imperial = "(a / 1.609)",
                    Units_Imperial = "MPH",
                    Expression_Metric = "",
                    Units_Metric = "KPH",
                    Cmd = "010D1",
                    Rate = 1000,
                    Decimals = 1,
                    Value_Min = 0,
                    Value_Max = 255,
                    isRxBytes = true,
                    Bytes = 4,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.VSS }
                };

                BluetoothCmd iat = new BluetoothCmd()
                {
                    Id = 2,
                    Name = "IAT",
                    Expression_Imperial = "((a-40)*9/5+32)",
                    Units_Imperial = "F",
                    Expression_Metric = "(a - 40)",
                    Units_Metric = "C",
                    Cmd = "010F1",
                    Rate = 1000,
                    Decimals = 1,
                    Value_Min = -40,
                    Value_Max = 215,
                    isRxBytes = true,
                    Bytes = 4,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.VSS }
                };

                BluetoothCmd map = new BluetoothCmd()
                {
                    Id = 3,
                    Name = "MAP",
                    Expression_Imperial = "(a*0.145)",
                    Units_Imperial = "PSI",
                    Expression_Metric = "(a)",
                    Units_Metric = "KPA",
                    Cmd = "010B1",
                    Rate = 1000,
                    Decimals = 1,
                    Value_Min = 0,
                    Value_Max = 255,
                    isRxBytes = true,
                    Bytes = 4,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.VSS }
                };

                BluetoothCmd tps = new BluetoothCmd()
                {
                    Id = 3,
                    Name = "TPS",
                    Expression_Imperial = "(a * 100 / 255)",
                    Units_Imperial = "%",
                    Expression_Metric = "(a * 100 / 255)",
                    Units_Metric = "%",
                    Cmd = "01111",
                    Rate = 500,
                    Decimals = 0,
                    Value_Min = 0,
                    Value_Max = 100,
                    isRxBytes = true,
                    Bytes = 1,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.TPS }
                };

                BluetoothCmd maf = new BluetoothCmd()
                {
                    Id = 4,
                    Name = "MAF",
                    Expression_Imperial = "(a * 1)",
                    Units_Imperial = "G/S",
                    Expression_Metric = "(a * 1)",
                    Units_Metric = "G/S",
                    Cmd = "010D1",
                    Rate = 1000,
                    Decimals = 1,
                    Value_Min = 0,
                    Value_Max = 10000,
                    isRxBytes = true,
                    Bytes = 1,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.MAF }
                };

                BluetoothCmd rpm = new BluetoothCmd()
                {
                    Id = 5,
                    Name = "RPM",
                    Expression_Imperial = "(a * 1)",
                    Units_Imperial = "x1000",
                    Expression_Metric = "(a * 1)",
                    Units_Metric = "x1000",
                    Cmd = "010C1",
                    Rate = 500,
                    Decimals = 0,
                    Value_Min = 0,
                    Value_Max = 10000,
                    isRxBytes = true,
                    Bytes = 1,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.DEFAULT }
                };

                BluetoothCmd mpg = new BluetoothCmd()
                {
                    Id = 6,
                    Name = "Mileage",
                    Expression_Imperial = "(14.7*b*1740.572)/(3600*c/100)",
                    Units_Imperial = "MPG",
                    Expression_Metric = "(14.7*b*1740.572)/(3600*c/100)",
                    Units_Metric = "KPG",
                    Cmd = "",
                    Rate = 2000,
                    Decimals = 1,
                    Value_Min = 0,
                    Value_Max = 100,
                    isRxBytes = false,
                    Bytes = 0,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.MPG, BlueToothCmds.COMMAND_TYPE.VSS, BlueToothCmds.COMMAND_TYPE.MAF }
                };

                BluetoothCmd fuel_system_status = new BluetoothCmd()
                {
                    Id = 7,
                    Name = "fuel system status",
                    Cmd = "01031",
                    Value_Max = 300,
                    isRxBytes = true,
                    Bytes = 1,
                    isSelected = false,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.DEFAULT }
                };

                BluetoothCmd fuel_pressure = new BluetoothCmd()
                {
                    Id = 8,
                    Name = "fuel pressure",
                    Expression_Imperial = "(100/128 * a - 100)",
                    Cmd = "010A1",
                    Rate = 500,
                    Decimals = 0,
                    Value_Min = 0,
                    Value_Max = 765,
                    isRxBytes = true,
                    Bytes = 1,
                    isSelected = true,
                    Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.DEFAULT }
                };

                connection.CreateTable<BluetoothCmd>();
                int rows = connection.Table<BluetoothCmd>().Count();

                if (rows <= 0)
                {
                    //connection.Insert(ign);
                    connection.Insert(coolant_temp);
                    connection.Insert(vss);
                    connection.Insert(tps);
                    connection.Insert(maf);
                    connection.Insert(rpm);
                    connection.Insert(mpg);
                    connection.Insert(fuel_system_status);
                    connection.Insert(fuel_pressure);
                }
            }
        }
    }
}
