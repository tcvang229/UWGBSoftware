#region Using Statements
using System;
using System.Collections.Generic;
using System.Globalization;
#endregion

namespace M.OBD2
{
    public class Dtc : List<string>
    {
        #region Declarations

        private readonly BluetoothCmd DTC_CLEAR;
        private readonly BluetoothCmd DTC_READ;

        private const string PID_ID = "PID:";
        private const string PID_PARSE = "BE3FA811";
        private const string PID_FORMATTER = "X2";
        private const uint PID_MASK = 0x80000000;

        private struct EngineTest
        {
            public bool Available;
            public bool Passed;
        }

        private struct EngineStatus
        {
            public bool MILActive;
            public int TroubleCodeCount;
            public EngineTest Misfire;
            public EngineTest FuelSystem;
            public EngineTest Components;
            public EngineTest Reserved;
            public EngineTest Catalyst;
            public EngineTest CatalystHeated;
            public EngineTest EvapSystem;
            public EngineTest SecondaryAirSystem;
            public EngineTest ACRefrigerant;
            public EngineTest O2Sensor;
            public EngineTest O2SensorHeater;
            public EngineTest EGRSystem;
        }
        private EngineStatus VehicleStatus;

        private readonly List<string> lst0103;

        #endregion

        /// <summary>
        ///  Initializes unique DTC commands
        /// </summary>

        public Dtc()
        {
        lst0103 = new List<string>();
        VehicleStatus = new EngineStatus();

        DTC_READ = new BluetoothCmd()
            {
                Cmd = "01051",
                Rate = 10000,
                isRxBytes = true,
                Bytes = 1
            };

            DTC_CLEAR = new BluetoothCmd()
            {
                Cmd = "04",
                Rate = 5000,
                isRxBytes = true,
                Bytes = 1,
                Command_Types = new[] { BlueToothCmds.COMMAND_TYPE.DTC_CLEAR }
            };
        }

        /// <summary>
        /// Build list of an engines available pid states
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>

        public  bool ProcessPids(int offset)
        {
            Clear();

            try
            {
                uint validPIDS = uint.Parse(PID_PARSE, NumberStyles.HexNumber);
                uint mask = PID_MASK;
                for (int pid = 1; pid <= 0x20; pid++)
                {
                    if ((validPIDS & mask) != 0)
                    {
                        Add(PID_ID + (pid + offset).ToString(PID_FORMATTER));
                    }

                    mask >>= 1;
                }

                return true;
            }
            catch (Exception ex) 
            {
                // ToDo: store error?
                return false;
            }
        }
    }
}
