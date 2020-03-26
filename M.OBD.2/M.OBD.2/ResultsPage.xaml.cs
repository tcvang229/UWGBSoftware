using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M.OBD2;
using Xamarin.Forms;

namespace M.OBD
{
    public partial class ResultsPage : ContentPage
    {
        private static Bluetooth oBluetooth;
        private static ProcessItems ProcessItems;
        private BlueToothCmds oBlueToothCmds;
        private DataTemplate CellTemplate;
        private UserSetting oUserSetting;

        private bool isTimerRun;
        private const int TIMER_UPDATE = 25;       // Update timer iteration delay in ms

        public ResultsPage()
        {
            InitializeComponent();

            InitListView();
            InitUserSettings();

            // ToDo: pass user settings value or null as default to invoke a users selection
            // ToDo: Remove test flag
            OpenBluetooth("OBDII", "00:1D:A5:05:4F:05", true);
        }

        #region User Settings

        private void InitUserSettings()
        {
            oUserSetting = new UserSetting();
        }

        #endregion

        #region Processing

        public void RunProcesses()
        {
            isTimerRun = true;
            DateTime dtCurrent = DateTime.UtcNow;

            foreach (BluetoothCmd bcmd in oBlueToothCmds)
            {
                bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
            }

            Device.StartTimer
            (
                TimeSpan.FromMilliseconds(TIMER_UPDATE), () =>
                {
                    foreach (BluetoothCmd bcmd in oBlueToothCmds)
                    {
                        dtCurrent = DateTime.UtcNow;

                        if (dtCurrent >= bcmd.dtNext)
                        {
                            RunProcess(bcmd, oBluetooth.isTestMode());

                            bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
                            //Debug.WriteLine("Process:" + bcmd.Name);
                        }
                    }

                    return isTimerRun;
                }
            );
        }

        private static void RunProcess(BluetoothCmd bcmd, bool isTestMode)
        {
            if (bcmd.CmdBytes != null && bcmd.CmdBytes.Length != 0)
            {
                if (!isTestMode)
                {
                    Task.Run(async () =>
                    {
                        if (await oBluetooth.SendCommandAsync(bcmd))
                            UpdateProcessItem(bcmd);
                        else
                            Debug.WriteLine("Process: {0} {1}", bcmd.Name, Bluetooth.GetStatusMessage());
                    });
                }
                else
                {
                    if (oBluetooth.SendCommandAsync_Test(bcmd))
                        UpdateProcessItem(bcmd);
                    else
                        Debug.WriteLine("Process: {0} {1}", bcmd.Name, Bluetooth.GetStatusMessage());
                }
            }
        }

        private static void UpdateProcessItem(BluetoothCmd bcmd)
        {
            // ToDo: Add direct ProcessItem reference to BluetoothCmd objects for faster iteration?

            foreach (ProcessItem pi in ProcessItems.Where(pi => pi.id == bcmd.Id))
            {
                pi.Value = bcmd.value.ToString();
                break;
            }

            //Debug.WriteLine("Process: {0} Rx: {1} RxValue: {2} Value: {3}", bcmd.Name,
            //    bcmd.Response, (bcmd.isRxBytes) ? bcmd.rxvalue : -1, bcmd.value);
        }

        #endregion

        #region Listview Related

        private void InitListView()
        {
            ProcessItems = new ProcessItems();
            CellTemplate = new DataTemplate(typeof(ProcessCell));
            lvwResults.ItemTemplate = CellTemplate;
            lvwResults.ItemsSource = ProcessItems;
            //lvwResults.Header = "Results";
        }

        private static void InitListViewItems(BlueToothCmds oBthCmds)
        {
            ProcessItems.Clear();

            foreach (BluetoothCmd bthCmd in oBthCmds)
            {
                AddListViewItem(bthCmd);
            }

            UpdateListViewItems();
        }

        private static void UpdateListViewItems()
        {
            if (ProcessItems == null || ProcessItems.Count == 0)
                return;

            // ToDo: any item ui changes here
            foreach (ProcessItem pi in ProcessItems)
            {
                //pi.ImageSource = "Tools_icon";
            }
        }

        private static void AddListViewItem(BluetoothCmd bthCmd)
        {
            if (!string.IsNullOrEmpty(bthCmd.Name))
            {
                ProcessItems.Add
                (
                    new ProcessItem
                    {
                        Name = bthCmd.Name,
                        Value = string.Empty,
                        Units = bthCmd.GetUnits(),
                        id = bthCmd.Id
                    }
                );
            }
        }

        #endregion

        #region Bluetooth Connection Related

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
                if (!oBluetooth.isTestMode())
                    return;
            }

            if (!Bluetooth.CheckAdapterEnabled()) // Check if bluetooth is enabled on this device: display message and return on failure
            {
                // ToDo: open OS settings page?
                DisplayMessage(Bluetooth.GetStatusMessage());
                if (!oBluetooth.isTestMode())
                    return;
            }

            oBluetooth = new Bluetooth(true, isTest); // Create connection object

            if (!oBluetooth.LoadPairedDevices()) // Attempt to load paired devices: display message and return on failure
            {
                DisplayMessage(Bluetooth.GetStatusMessage());

                if (!oBluetooth.isTestMode())
                    return;
            }

            if (!oBluetooth.CheckPairedDevices()) // Check if there are paired devices available: display message and return on failure
            {
                // ToDo: open OS settings page?
                DisplayMessage(Bluetooth.GetStatusMessage());

                if (!oBluetooth.isTestMode())
                    return;
            }

            if (!await oBluetooth.OpenPairedDevice(name, address)) // Attempt to open paired device: if failed get list of paired devices
            {
                List<BluetoothConnection> bcs = oBluetooth.GetPairedDevices();

                // ToDo: populate a listview and let user select the OBDII device
                // Retry oBth.OpenPairedDevice(name, address);

                if (!oBluetooth.isTestMode())
                    return;
            }

            // Success! //////////

            // Load some test commands and run processing loop

            oBlueToothCmds = new BlueToothCmds();
            oBlueToothCmds.CreateTestCommands(oUserSetting.GetUserUnits());

            InitListViewItems(oBlueToothCmds);
            RunProcesses();
        }

        private async void DisplayMessage(string message)
        {
            await DisplayAlert("Alert", message, "OK");
        }

        #endregion
    }
}
