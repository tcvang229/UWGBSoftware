using M.OBD._2;
using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace M.OBD2
{
    public static class BluetoothCommandsInitializer
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
                connection.CreateTable<BluetoothCmd>();
                List<BluetoothCmd> commands_list = CreateList();

                foreach (BluetoothCmd command in commands_list)
                {
                    connection.Insert(command);
                }
            }
        }

        public static List<BluetoothCmd> CreateList()
        {
            BluetoothCmd coolant_temp = new BluetoothCmd()
            {
                Id = 1,
                Name = "coolant temp",
                Units_Imperial = "F",
                Units_Metric = "C",
                Cmd = "01051",
                Rate = 3000,
                Decimals = 0,
                Expression_Imperial = "((a-40)*9/5+32)",
                Expression_Metric = "(a - 40)",
                Bytes = 4,
                isRxBytes = true,
                isSelected = false,
                Value_Min = -40,
                Value_Max = 215,
            };

            BluetoothCmd vss = new BluetoothCmd()
            {
                Id = 2,
                Name = "VSS",
                Units_Imperial = "MPH",
                Units_Metric = "KPH",
                Cmd = "010D1",
                Rate = 1000,
                Decimals = 1,
                Expression_Imperial = "(a / 1.609)",
                Expression_Metric = "",
                Bytes = 4,
                isRxBytes = true,
                sCommand_Types = "2",
                isSelected = true,
                Value_Min = 0,
                Value_Max = 255,
            };

            BluetoothCmd iat = new BluetoothCmd()
            {
                Id = 3,
                Name = "IAT",
                Units_Imperial = "F",
                Units_Metric = "C",
                Cmd = "010F1",
                Rate = 1000,
                Decimals = 1,
                Expression_Imperial = "((a-40)*9/5+32)",
                Expression_Metric = "(a - 40)",
                Bytes = 4,
                isRxBytes = true,
                sCommand_Types = "2",
                isSelected = false,
                Value_Min = -40,
                Value_Max = 215,
            };

            BluetoothCmd map = new BluetoothCmd()
            {
                Id = 4,
                Name = "MAP",
                Units_Imperial = "PSI",
                Units_Metric = "KPA",
                Cmd = "010B1",
                Rate = 1000,
                Decimals = 1,
                Expression_Imperial = "(a*0.145)",
                Expression_Metric = "(a)",
                Bytes = 4,
                isRxBytes = true,
                sCommand_Types = "2",
                isSelected = false,
                Value_Min = 0,
                Value_Max = 255,
            };

            BluetoothCmd tps = new BluetoothCmd()
            {
                Id = 5,
                Name = "TPS",
                Units_Imperial = "%",
                Units_Metric = "%",
                Cmd = "01111",
                Rate = 500,
                Decimals = 0,
                Expression_Imperial = "(a * 100 / 255)",
                Expression_Metric = "(a * 100 / 255)",
                Bytes = 1,
                isRxBytes = true,
                sCommand_Types = "0",
                isSelected = false,
                Value_Min = 0,
                Value_Max = 100,
            };

            BluetoothCmd maf = new BluetoothCmd()
            {
                Id = 6,
                Name = "MAF",
                Units_Imperial = "G/S",
                Units_Metric = "G/S",
                Cmd = "010D1",
                Rate = 1000,
                Decimals = 1,
                Expression_Imperial = "(a * 1)",
                Expression_Metric = "(a * 1)",
                Bytes = 1,
                isRxBytes = true,
                sCommand_Types = "3",
                isSelected = false,
                Value_Min = 0,
                Value_Max = 10000,
            };

            BluetoothCmd rpm = new BluetoothCmd()
            {
                Id = 7,
                Name = "RPM",
                Units_Imperial = "x1000",
                Units_Metric = "x1000",
                Cmd = "010C1",
                Rate = 500,
                Decimals = 0,
                Expression_Imperial = "(a * 1)",
                Expression_Metric = "(a * 1)",
                Bytes = 1,
                isRxBytes = true,
                sCommand_Types = "0",
                isSelected = false,
                Value_Min = 0,
                Value_Max = 10000,
            };

            BluetoothCmd mpg = new BluetoothCmd()
            {
                Id = 8,
                Name = "miles per gallon",
                Units_Imperial = "MPG",
                Units_Metric = "KPG",
                Cmd = "",
                Rate = 2000,
                Decimals = 1,
                Expression_Imperial = "(14.7*b*1740.572)/(3600*c/100)",
                Expression_Metric = "(14.7*b*1740.572)/(3600*c/100)",
                Bytes = 0,
                isRxBytes = false,
                sCommand_Types = "0",
                isSelected = false,
                Value_Min = 0,
                Value_Max = 100,
            };

            BluetoothCmd fuel_system_status = new BluetoothCmd()
            {
                Id = 9,
                Name = "fuel system status",
                Units_Imperial = "",
                Units_Metric = "",
                Cmd = "01031",
                Rate = 5000,
                Decimals = 0,
                Expression_Imperial = "",
                Expression_Metric = "",
                Bytes = 4,
                isRxBytes = false,
                sCommand_Types = "0",
                isSelected = false,
                Value_Max = 0,
                Value_Min = 0,
            };

            BluetoothCmd fuel_pressure = new BluetoothCmd()
            {
                Id = 10,
                Name = "fuel pressure",
                Units_Imperial = "",
                Units_Metric = "",
                Cmd = "010A1",
                Rate = 500,
                Decimals = 0,
                Expression_Imperial = "(100/128 * a - 100)",
                Expression_Metric = "",
                Bytes = 4,
                isRxBytes = true,
                sCommand_Types = "0",
                isSelected = false,
                Value_Min = 0,
                Value_Max = 765,
            };

            List<BluetoothCmd> commands_list = new List<BluetoothCmd>();
            commands_list.Add(coolant_temp);
            commands_list.Add(vss);
            commands_list.Add(iat);
            commands_list.Add(map);
            commands_list.Add(tps);
            commands_list.Add(maf);
            commands_list.Add(rpm);
            commands_list.Add(mpg);
            commands_list.Add(fuel_system_status);
            commands_list.Add(fuel_pressure);
            commands_list.Add(ign);

            return commands_list;
        }
    }
}
