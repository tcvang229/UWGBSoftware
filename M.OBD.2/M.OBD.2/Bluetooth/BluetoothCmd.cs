#region Using Statements

using M.OBD._2;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace M.OBD2
{
    /// <summary>
    /// Bluetooth Command Process
    /// </summary>
    public class BluetoothCmd : ProcessValue
    {
        //SQL Attributes for ID
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        // Processes display name
        [MaxLength(100)]
        public string Name { get; set; }

        // Displayed units value per type ex. psi, vdc, g/s
        [MaxLength(10)]
        public string Units_Imperial { get; set; }

        [MaxLength(10)]
        public string Units_Metric { get; set; }

        // Associated AT command ex. 104030
        [MaxLength(10)]
        public string Cmd { get; set; }

        // AT command Tx rate in ms
        public int Rate { get; set; }

        // Rounding and displayed decimal formatting
        public int Decimals { get; set; }

        // Associated math expression per unit type including variables ex. (a*1) ...
        public string Expression_Imperial { get; set; }

        public string Expression_Metric { get; set; }

        // Number of bytes expected in a response
        public int Bytes { get; set; }

        //  If this command expects a response in bytes or a string 
        public bool isRxBytes { get; set; }

        // Command type enum array in Json encoded format ex: "{2}" or  "{2,3,4}" ...
        public string sCommand_Types { get; set; }

        public string sSelection_Types { get; set; }

        // If the user has selected this process
        public bool isSelected { get; set; }

        // Process min value contraint
        public double Value_Min { get; set; }

        // Process max value constraint
        public double Value_Max { get; set; }

        // Non DB Values /////

        // Pre generated command bytes for faster iteration
        [Ignore]
        public byte[] CmdBytes { get; set; }

        // Associated Command_Type - multiple indicates multi values used in a given Expression string 
        // Ex: Expression = "(a*b*1740.572)/(3600*c/100)", Command_Types = { COMMAND_TYPE.MPG, COMMAND_TYPE.VSS, COMMAND_TYPE.MAF }
        [Ignore]
        public BlueToothCmds.COMMAND_TYPE[] Command_Types { get; set; }

        // Command selection state Ex. User, Process ...
        [Ignore]
        public BlueToothCmds.SELECTION_TYPE Selection_Type { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BlueToothCmds : List<BluetoothCmd>
    {
        #region Declarations

        // Maximum user process selections
        public const int MAX_COMMANDS = 10;
        // Duplicated specifier types for faster iteration
        public const string SCOMMAND_SPECIFIER = ",";
        public const char COMMAND_SPECIFIER = ',';

        // Process command type 
        public enum COMMAND_TYPE
        {
            DEFAULT,
            AFR,
            VSS,
            MAF,
            MPG,
            TPS,
            DTC_READ,
            DTC_CLEAR
        }

        // Current selection state of a process
        public enum SELECTION_TYPE
        {
            NONE,   // No selection
            USER,   // User selected
            PROCESS,    // Process selected
            USER_PROCESS // User and Process selected
        }

        #endregion

        #region Initialization

        // see if these methods are needed or not
        // MARKER 4

        public void InitExpressions(UserSetting.UNIT_TYPE Unit_Type)
        {
            foreach (BluetoothCmd b in this) // Init expressions
            {
                b.InitExpression(b, Unit_Type);
            }
        }

        public void InitCommandBytes()
        {
            foreach (BluetoothCmd b in this.Where(b => !string.IsNullOrEmpty(b.Cmd)))
            {
                b.CmdBytes = Encoding.ASCII.GetBytes(b.Cmd + Bluetooth.LINE_BREAK);
            }
        }

        public static byte[] GetCommandBytes(string cmd)
        {
            return string.IsNullOrEmpty(cmd) ? null : Encoding.ASCII.GetBytes(cmd + Bluetooth.LINE_BREAK);
        }

        #endregion

        #region DB

        //public BlueToothCmds(UserSetting user_setting)
        public BlueToothCmds()
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {
                // create the new database table here
                // MARKER 5
                //connection.CreateTable<BluetoothCmd>();

                try
                {
                    int rows = connection.Table<BluetoothCmd>().Count();
                    System.Diagnostics.Debug.WriteLine("----ROWS----");
                }
                catch (SQLite.SQLiteException e)
                {
                    System.Diagnostics.Debug.WriteLine("----ROWS EXCEPTION----");
                    BluetoothCommandsInitializer.Initialize();
                }
            }
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

        #endregion

        #region Command Type Related

        // Array to Json serialization method for handling db arrays
        public static string CommandTypesToJson(COMMAND_TYPE[] Command_Types)
        {
            try
            {
                return JsonConvert.SerializeObject(Command_Types);
            }
            catch
            {
            }

            return null;
        }

        // Json to array deserialize method for handling db arrays
        public static COMMAND_TYPE[] JsonToCommandTypes(string sCommand_Types)
        {
            if (!string.IsNullOrEmpty(sCommand_Types))
            {
                try
                {
                    return JsonConvert.DeserializeObject<COMMAND_TYPE[]>(sCommand_Types);
                }
                catch
                {
                }
            }
            return null;
        }

        #endregion

        #region Test Related
        public void RetrieveCommands(UserSetting.UNIT_TYPE Unit_Type, bool isInitialize)
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {
                // TO DO:
                // need to convert RECORD -> BluetoothCmd
                var commands = connection.Table<BluetoothCmd>();
                System.Diagnostics.Debug.WriteLine(commands.Count().ToString());

                foreach (var command in commands)
                {
                    System.Diagnostics.Debug.WriteLine("Id: " + command.Id.ToString());
                }

                /*foreach (var command in commands)
                {
                    // need to do some conversion
                    // before adding RECORD -> LIST
                    Add(command);
                    System.Diagnostics.Debug.WriteLine(command.Id.ToString());
                }*/

                if (isInitialize)
                {
                    InitCommandBytes();
                    InitExpressions(Unit_Type);
                }
            }
        }

        /*public void CreateTestCommands(UserSetting.UNIT_TYPE Unit_Type, bool isInitialize)
        {
            // Format 01##01
            // 01 = Service
            // https://en.wikipedia.org/wiki/OBD-II_PIDs#Service_01

            Add(new BluetoothCmd()
            {
                Id = 0,
                Name = "IGN",
                Expression_Imperial = "",
                Units_Imperial = "",
                Cmd = "ATIGN",
                Rate = 1000,
                isRxBytes = false,
                isSelected = true,
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
            {
                Id = 1,
                Name = "TEMP",
                Expression_Imperial = "(a * 100 / 255)",
                Units_Imperial = "F",
                Expression_Metric = "(a * 100 / 255)",
                Units_Metric = "C",
                Cmd = "01051",
                Rate = 2000,
                Decimals = 0,
                Value_Min = -60,
                Value_Max = 300,
                isRxBytes = true,
                Bytes = 1,
                isSelected = true,
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
            {
                Id = 2,
                Name = "VSS",
                Expression_Imperial = "(a * 1)",
                Units_Imperial = "MPH",
                Expression_Metric = "(a * 1)",
                Units_Metric = "KPH",
                Cmd = "010D1",
                Rate = 1000,
                Decimals = 1,
                Value_Min = 0,
                Value_Max = 300,
                isRxBytes = true,
                Bytes = 1,
                isSelected = true,
                Command_Types = new[] { COMMAND_TYPE.VSS }
            });

            Add(new BluetoothCmd()
            {
                Id = 3,
                Name = "TPS",
                Expression_Imperial = "(a * 1)",
                Units_Imperial = "%",
                Expression_Metric = "(a * 1)",
                Units_Metric = "%",
                Cmd = "010D1",
                Rate = 1000,
                Decimals = 0,
                Value_Min = 0,
                Value_Max = 100,
                isRxBytes = true,
                Bytes = 1,
                isSelected = false,
                Command_Types = new[] { COMMAND_TYPE.TPS }
            });

            Add(new BluetoothCmd()
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
                Command_Types = new[] { COMMAND_TYPE.MAF }
            });

            Add(new BluetoothCmd()
            {
                Id = 5,
                Name = "RPM",
                Expression_Imperial = "(a * 1)",
                Units_Imperial = "x1000",
                Expression_Metric = "(a * 1)",
                Units_Metric = "x1000",
                Cmd = "010D1",
                Rate = 500,
                Decimals = 0,
                Value_Min = 0,
                Value_Max = 10000,
                isRxBytes = true,
                Bytes = 1,
                isSelected = false,
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
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
                Command_Types = new[] { COMMAND_TYPE.MPG, COMMAND_TYPE.VSS, COMMAND_TYPE.MAF }
            });

            if (isInitialize)
            {
                InitCommandBytes();
                InitExpressions(Unit_Type);
            }
        }*/

        #endregion
    }
}
