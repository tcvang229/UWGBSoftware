using System;
using System.Collections.Generic;
using System.Text;

namespace M.OBD2
{
    public abstract class Units
    {
        protected Units()
        { }

        public abstract void InitType(BluetoothCmd bthcmd);

        public static string CheckString(string svalue)
        {
            return (String.IsNullOrEmpty(svalue)) ? String.Empty : svalue;
        }
    }
}
