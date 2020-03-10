using System;
using System.Collections.Generic;
using M.OBD._2;
using SQLite;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace M.OBD2
{
    
    public class BluetoothCmd
    {
        //SQL Attributes for ID
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string Units { get; set; }

        public bool isImperial { get; set; }

        [MaxLength(10)]
        public string Cmd { get; set; }

        public int Rate { get; set; }
        public int Decimals { get; set; }
        public string Expression { get; set; }

        //not sure how the db will handle this type?
        public BlueToothCmds.COMMAND_TYPE[] Command_Types { get; set; }
        public DateTime dtNext { get; set; }

        public BluetoothCmd()
        {
        }
    }

    public class BlueToothCmds : List<BluetoothCmd>
    {
        public enum COMMAND_TYPE
        {
            DEFAULT,
            AFR,
            VSS,
            MAF,
            MPG
        }

        //method to create a bluetooth command and insert it into the DB
        private async void createRecords(BluetoothCmd cmd)
        {
            //create the sql lite connection and wrap in curly braces
            //so it automatically closes the connection when it is done
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {
                //create the table if it already doesn't exist
                connection.CreateTable<BluetoothCmd>();

                //variable to hold if the insert was successful or not
                int rows = connection.Insert(cmd);

                if (rows > 0)
                {
                    Console.WriteLine("Bluetooth Command was successfully added to DB");
                }
                else
                {
                    Console.WriteLine("Error, Bluetooth Command was not successfully added to DB");
                }
            }


        }

        public async Task getAllCommandsAsync()
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {

                //get the entire table
                var bluetoothCmd = connection.Table<BluetoothCmd>();

                //example linq query 
                //use a linq query to get the commands from the bluetooth commands table
                /*
                var commands = (from c in bluetoothCmd
                                orderby c.CategoryID
                                select c.CategoryName).Distinct().ToList();
                 */
            }

        }

        public BlueToothCmds() // ToDo: load commands from Db
        {
        //ToDo: //integrate with a User class to get their preferences??
        }

        public void CreateTestCommands()
        {
            Add(new BluetoothCmd()
            {
                Id = 0,
                Name = "RPM",
                Units = "",
                isImperial = true,
                Cmd = "",
                Rate =  200,
                Decimals = 0,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
            {
                Id = 1,
                Name = "VOLTS",
                Units = "VDC",
                isImperial = true,
                Cmd = "",
                Rate = 200,
                Decimals = 0,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
                {
                    Id = 2,
                    Name = "AFR",
                    Units = "",
                    isImperial = true,
                    Cmd = "",
                    Rate = 1000,
                    Decimals = 2,
                    Expression = "a*1",
                    Command_Types = new[] { COMMAND_TYPE.AFR }
            });

            Add(new BluetoothCmd()
            {
                Id = 3,
                Name = "VSS",
                Units = "Mph",
                isImperial = true,
                Cmd = "",
                Rate = 1000,
                Decimals = 2,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.VSS }
            });

            Add(new BluetoothCmd()
            {
                Id = 4,
                Name = "MAF",
                Units = "g/s",
                isImperial = true,
                Cmd = "",
                Rate = 500,
                Decimals = 2,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.MAF }
            });

            Add(new BluetoothCmd()
            {
                Id = 5,
                Name = "MPG",
                Units = "",
                isImperial = true,
                Cmd = null,
                Rate = 5000,
                Decimals = 1,
                Expression = "(a*b*1740.572)/(3600*c/100",
                Command_Types = new[] { COMMAND_TYPE.MPG, COMMAND_TYPE.AFR, COMMAND_TYPE.VSS, COMMAND_TYPE.MAF }
            });
        }
    }
}
