#region Using Statements
using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace M.OBD2
{
    public class ProcessValue : UnitFactory
    {
        #region Declarations

        public DateTime dtNext { get; set; }
        public StringBuilder sbResponse { get; set; }
        public string Response { get; set; }
        public ulong tx_fail { get; set; }
        public ulong tx_good { get; set; }
        public ulong rx_fail { get; set; }
        public ulong rx_good { get; set; }

        public int rxvalue { get; set; }  // Received raw value
        public double value { get; set; }       // Processed value
        public bool isValid { get; set; }
        public Expression e;
        public string sExpression { get; set; }

        private List<Argument> arguments;
        private string LastExpression;
        private string sUnits;
        private double min;
        private double max;
        private int decimals;

        #endregion

        #region Initialization

        public ProcessValue()
        {
            sbResponse = new StringBuilder();
        }

        /// <summary>
        ///  Initializes a given commands math expression based on supplied unit type 
        /// </summary>
        /// <param name="bthcmd"></param>
        /// <param name="Unit_Type"></param>

        public void InitExpression(BluetoothCmd bthcmd, UserSetting.UNIT_TYPE Unit_Type)
        {
            if (bthcmd.Command_Types == null || bthcmd.Command_Types.Length == 0)
                return;

            InitUnitType(bthcmd, Unit_Type);

            if (string.IsNullOrEmpty(sExpression))
                return;

            string varvalue;
            e = null;
            arguments = null;

            e = new Expression(sExpression.Trim());
            arguments = new List<Argument>();

            for (int idx = 0; idx < bthcmd.Command_Types.Length; idx++)
            {
                varvalue = ProcessValues.GetVariable(idx);
                arguments.Add(new Argument(varvalue));
                e.addArguments(arguments[idx]);
                //arguments[idx].setArgumentValue(0); // ToDo: initial value?
            }

            min = bthcmd.Value_Min;
            max = bthcmd.Value_Max;
            decimals = bthcmd.Decimals;
        }

        #endregion

        #region Calculations

        public bool Calculate()
        {
            return Calculate(this);
        }

        private static bool Calculate(ProcessValue p)
        {
            if (p.e == null || p.arguments.Count == 0)
                return false;

            if (p.arguments.Count == 1) // Single variable
            {
                p.arguments[0].setArgumentValue(p.rxvalue);
                p.value = p.e.calculate();
                p.value = Math.Round(p.value, p.decimals);

                if (p.value < p.min)
                    p.value = p.min;
                if (p.value > p.max)
                    p.value = p.max;

                // ToDo: remove for release
                p.LastExpression = p.e.getExpressionString();
            }
            else // Multi variable
            {

            }

            return true;
        }

        #endregion

        #region Gets/Sets

        public string GetUnits()
        {
            return sUnits;
        }

        public void SetUnits(string svalue)
        {
            sUnits = svalue;
        }

        public void SetExpression(string svalue)
        {
            sExpression = svalue;
        }

        #endregion
    }

    #region Helper Class

    public class ProcessValues
    {
        private static readonly char[] VARIABLES = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        public static string GetVariable(int index)
        {
            if (index < 0 || index > VARIABLES.Length)
                return null;

            return VARIABLES[index].ToString();
        }
    }

    #endregion
}
