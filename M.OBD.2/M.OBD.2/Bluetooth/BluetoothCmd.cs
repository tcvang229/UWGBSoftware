using System;
using System.Collections.Generic;
using M.OBD._2;
using SQLite;
using Xamarin.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.OS;
using Org.Json;
using Newtonsoft.Json;

namespace M.OBD2
{
    
    public class BluetoothCmd : ProcessValue
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
        public int Bytes { get; set; }
        public bool isRxBytes { get; set; }
        public string sCommand_Types { get; set; } // Command type enum array in Json encoded format ex: "{2}" or  "{2,3,4}" ...
        public bool isSelected { get; set; }                // If user has selected 

        // Non DB Values
        public byte[] CmdBytes { get; set; }    // Pre generated command bytes for faster iteration from the Cmd string
        // Associated Command_Type - multiple indicates multi values used in a given Expression string 
        // Ex: Expression = "(a*b*1740.572)/(3600*c/100)", Command_Types = { COMMAND_TYPE.MPG, COMMAND_TYPE.VSS, COMMAND_TYPE.MAF }
        public BlueToothCmds.COMMAND_TYPE[] Command_Types { get; set; }
        public BlueToothCmds.SELECTION_TYPE Selection_Type { get; set; } // Command selection state Ex. User, Process ...

        public BluetoothCmd()
        {
        }
    }

    public class BlueToothCmds : List<BluetoothCmd>
    {
        #region Declarations

        public const int MAX_COMMANDS = 10; // Maximum user process selections
        public const string SCOMMAND_SPECIFIER = ","; // Duplicated specifier types for faster iteration
        public const char COMMAND_SPECIFIER = ',';

        public enum COMMAND_TYPE // Process command type 
        {
            DEFAULT,
            AFR,
            VSS,
            MAF,
            MPG,
            TPS
        }

        public enum SELECTION_TYPE // Current selection state of a process
        {
            NONE,   // No selection
            USER,   // User selected
            PROCESS,    // Process selected
            USER_PROCESS // User and Process selected
        }

        #endregion

        #region Initialization

        public void InitExpressions()
        {

        }

        public void InitCommandBytes()
        {
            foreach (BluetoothCmd bthcmd in this.Where(bthcmd => !string.IsNullOrEmpty(bthcmd.Cmd)))
            {
                bthcmd.CmdBytes = Encoding.ASCII.GetBytes(bthcmd.Cmd + Bluetooth.LINE_BREAK);
            }
        }

        #endregion

        #region DB

        public BlueToothCmds() // ToDo: load commands from Db
        {
            //ToDo: //integrate with a User class to get their preferences??
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

        public static string CommandTypesToJson(COMMAND_TYPE[] Command_Types) // Array to Json serialization method for handling db arrays
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

        public static COMMAND_TYPE[] JsonToCommandTypes(string sCommand_Types) // Json to array deserialize method for handling db arrays
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

        #region Test

        public void CreateTestCommands()
        {
            // Format 01##01
            // 01 = Service
            // https://en.wikipedia.org/wiki/OBD-II_PIDs#Service_05

            Add(new BluetoothCmd()
            {
                Id = 2,
                Name = "IGN",
                Units = "",
                isImperial = true,
                Cmd = "ATIGN",
                Rate = 1000,
                Decimals = 0,
                isRxBytes = false,
                Expression = "",
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
            {
                Id = 3,
                Name = "TEMP",
                Units = "%",
                isImperial = true,
                Cmd = "01051",
                Rate = 2000,
                Decimals = 0,
                isRxBytes = true,
                Bytes = 1,
                Expression = "(a * 100 / 255)",
                Command_Types = new[] { COMMAND_TYPE.DEFAULT }
            });

            Add(new BluetoothCmd()
            {
                Id = 3,
                Name = "VSS",
                Units = "Mph",
                isImperial = true,
                Cmd = "010D1",
                Rate = 1000,
                Decimals = 1,
                isRxBytes = true,
                Bytes = 1,
                Expression = "a*1",
                Command_Types = new[] { COMMAND_TYPE.VSS }
            });

            //Add(new BluetoothCmd()
            //{
            //    Id = 5,
            //    Name = "MPG",
            //    Units = "",
            //    isImperial = true,
            //    Cmd = null,
            //    Rate = 5000,
            //    Decimals = 1,
            //    Expression = "(a*b*1740.572)/(3600*c/100)",
            //    Command_Types = new[] { COMMAND_TYPE.MPG, COMMAND_TYPE.VSS, COMMAND_TYPE.MAF }
            //});

            InitCommandBytes();

            foreach (BluetoothCmd b in this) // Init expressions
            {
                b.InitExpression(b.Expression, b.Command_Types);
            }
        }
        #endregion
    }
}
