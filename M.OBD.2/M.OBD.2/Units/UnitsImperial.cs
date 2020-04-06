using System;
using System.Collections.Generic;
using System.Text;

namespace M.OBD2
{
    public class UnitsImperial : Units
    {
        public UnitsImperial()
        {
        }

        public override void InitType(BluetoothCmd bthcmd)
        {
            bthcmd.SetUnits(bthcmd.Units_Imperial);
            bthcmd.SetExpression(bthcmd.Expression_Imperial);
        }
    }
}
