using System;
using System.Collections.Generic;
using System.Text;

namespace M.OBD2
{
    public class UnitsMetric : Units
    {
        public UnitsMetric()
        {
        }

        public override void InitType(BluetoothCmd bthcmd)
        {
            bthcmd.SetUnits(CheckString(bthcmd.Units_Metric));
            bthcmd.SetExpression(CheckString(bthcmd.Expression_Metric));
        }
    }
}
