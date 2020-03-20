using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace M.OBD2
{
    public class Process
    {
        private readonly BlueToothCmds oBlueToothCmds;
        private readonly Bluetooth oBluetooth;

        public Process(Bluetooth oBluetooth, BlueToothCmds oBlueToothCmds)
        {
            this.oBluetooth = oBluetooth;
            this.oBlueToothCmds = oBlueToothCmds;
        }

        public async Task RunProcesses()
        {
            await RunProcess();
        }

        public async Task RunProcess()
        {
            DateTime dtCurrent = DateTime.UtcNow;

            foreach (BluetoothCmd bcmd in oBlueToothCmds)
            {
                bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
            }

            // ToDo: implement UI start/stop control 
            while (true) 
            {
                foreach (BluetoothCmd bcmd in oBlueToothCmds)
                {
                    dtCurrent = DateTime.UtcNow;

                    if (dtCurrent >= bcmd.dtNext)
                    {
                        if (bcmd.CmdBytes != null && bcmd.CmdBytes.Length != 0)
                        {
                            if (await oBluetooth.SendCommandAsync(bcmd))
                                Debug.WriteLine("Process: {0} Rx: {1} RxValue: {2} Value: {3}", bcmd.Name,  bcmd.Response,  (bcmd.isRxBytes) ? bcmd.rxvalue : -1, bcmd.value);
                            else
                                Debug.WriteLine("Process: {0} {1}", bcmd.Name, Bluetooth.GetStatusMessage());
                        }
                        
                        bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
                        Debug.WriteLine("Process:" + bcmd.Name);
                    }
                }
            }
        }
    }
}
