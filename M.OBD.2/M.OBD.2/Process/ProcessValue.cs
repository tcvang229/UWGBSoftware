using System;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using org.mariuszgromada.math.mxparser;

namespace M.OBD2
{
    public class ProcessValue
    {
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
        private List<Argument> arguments;
        private string LastExpression { get; set; }

        public ProcessValue()
        {
            sbResponse = new StringBuilder();
        }

        public void InitExpression(string Expression, BlueToothCmds.COMMAND_TYPE[] Command_Types)
        {
            if (string.IsNullOrEmpty(Expression) || Command_Types == null || Command_Types.Length == 0) // Validate
                return;

            e = new Expression(Expression.Trim());
            arguments = new List<Argument>();

            for (int idx = 0; idx < Command_Types.Length; idx++)
            {
                string varvalue = ProcessValues.GetVariable(idx);
                arguments.Add(new Argument(varvalue));
                e.addArguments(arguments[idx]);

                //arguments[idx].setArgumentValue(0); // ToDo: initial value?
            }

            // Test
            // double result = e.calculate();
            // string exp = e.getExpressionString();
        }

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

                // ToDo: remove for release
                p.LastExpression = p.e.getExpressionString();
            }

            return true;
        }
    }

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
}
