using SQLite;
using System;
using System.Collections.Generic;
using System.Text;


namespace M.OBD2
{
    public class BluetoothModel
    {
        //SQL Attributes for ID
        //[PrimaryKey, AutoIncrement]
        [PrimaryKey]
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

        // use numeric value of BluetoothCmd.COMMAND_TYPE[] enum
        // ex. "0123"
        // -> will convert to char[] then reference each char to
        // enum numeric value
        public string sCommand_Types { get; set; }

        // If the user has selected this process
        public bool isSelected { get; set; }

        // Process min value contraint
        public double Value_Min { get; set; }

        // Process max value constraint
        public double Value_Max { get; set; }
    }
}
