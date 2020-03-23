using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M.OBD2;
using Xamarin.Forms;


namespace M.OBD
{
    public partial class ResultsPage : ContentPage
    {
        public ResultsPage()
        {
            InitializeComponent();

            // ToDo: pass user settings value or null as default to invoke a users selection
            // ToDo: Remove test flag
            OpenBluetooth("OBDII", "00:1D:A5:05:4F:05", true);
        }

        /// <summary>
        /// Performs Bluetooth connection operations
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="isTest"></param>
        /// 
        private async void OpenBluetooth(string name, string address, bool isTest)
        {
            if (!Bluetooth.CheckAdapterPresent()) // Check if bluetooth is available on this device: display message and return on failure
            {
                DisplayMessage(Bluetooth.GetStatusMessage());
                return;
            }

            if (!Bluetooth.CheckAdapterEnabled()) // Check if bluetooth is enabled on this device: display message and return on failure
            {
                // ToDo: open OS settings page?
                DisplayMessage(Bluetooth.GetStatusMessage());
                return;
            }

            Bluetooth oBth = new Bluetooth(true, isTest); // Create connection object

            if (!oBth.LoadPairedDevices()) // Attempt to load paired devices: display message and return on failure
            {
                DisplayMessage(Bluetooth.GetStatusMessage());

                if (!oBth.isTestMode())
                    return;
            }

            if (!oBth.CheckPairedDevices()) // Check if there are paired devices available: display message and return on failure
            {
                // ToDo: open OS settings page?
                DisplayMessage(Bluetooth.GetStatusMessage());

                if (!oBth.isTestMode())
                    return;
            }

            if (!await oBth.OpenPairedDevice(name, address)) // Attempt to open paired device: if failed get list of paired devices
            {
                List<BluetoothConnection> bcs = oBth.GetPairedDevices();

                // ToDo: populate a listview and let user select the OBDII device
                // Retry oBth.OpenPairedDevice(name, address);

                if (!oBth.isTestMode())
                    return;
            }

            // Success! //////////

            // Load some test commands and run processing loop

            BlueToothCmds oBthCmds = new BlueToothCmds();
            oBthCmds.CreateTestCommands();

            Process oProcess = new Process(oBth, oBthCmds);
            await oProcess.RunProcesses();
        }

        private async void DisplayMessage(string message)
        {
            await DisplayAlert("Alert", message, "OK");
        }
    }
}
