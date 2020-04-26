#region Using statements

using M.OBD._2;
using M.OBD2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PCLExt.FileStorage;
using Xamarin.Forms;

#endregion

namespace M.OBD
{
    public partial class ResultsPage : ContentPage, IPageLoad
    {
        #region Declarations

        private readonly Bluetooth oBluetooth;
        private Logging oLogging;
        private static ProcessItems ProcessItems;
        private BlueToothCmds oBlueToothCmds;
        private BlueToothCmds oBlueToothCmds_Picker;
        private DataTemplate CellTemplate;
        private readonly UserSetting oUserSetting;

        private bool isTimerRun;
        private const int TIMER_UPDATE = 25;       // Update timer iteration delay in ms
        private bool isPickerProcessActive;
        private bool isPickerProcessSelected;
        private bool isPickerLogsActive;
        private bool isLogging;

        #endregion

        #region Page Initialization and Update

        public ResultsPage()
        {
            InitializeComponent();

            InitBluetooth(out oBluetooth);
            InitUserSettings(out oUserSetting);
            InitControls();
        }

        public void InitBluetooth(out Bluetooth bluetooth)
        {
            bluetooth = App.GetBluetooth();
        }

        public void InitUserSettings(out UserSetting usersetting)
        {
            usersetting = App.GetUserSetting();
        }

        public void InitControls()
        {
            btnConnect.Clicked += btnConnect_Clicked;
            btnDisconnect.Clicked += btnDisconnect_Clicked;
            btnLogOn.Clicked += btnLogOn_Clicked;
            btnLogOff.Clicked += btnLogOff_Clicked;
            btnLogList.Clicked += btnLogList_Clicked;
            btnSelect.Clicked += btnSelect_Clicked;
            Appearing += Page_Appearing;
            pkrProcess.SelectedIndexChanged += pkrProcess_SelectedIndexChanged;
            pkrProcess.Unfocused += pkrProcess_Unfocused;
            pkrLogs.Unfocused += pkrLogs_Unfocused;
            InitListView();
        }

        private void PkrLogs_Unfocused(object sender, FocusEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Page_Appearing(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateUserSettings();
        }

        public void UpdateControls()
        {
            btnConnect.IsEnabled = Bluetooth.isBluetoothDisconnected();
            btnDisconnect.IsEnabled = !Bluetooth.isBluetoothDisconnected();
            btnSelect.IsEnabled = Bluetooth.isBluetoothDisconnected();
            btnLogOn.IsEnabled = !Bluetooth.isBluetoothDisconnected() && !Logging.GetIsLogging();
            btnLogOff.IsEnabled = !Bluetooth.isBluetoothDisconnected() && Logging.GetIsLogging();
            //btnLogList.IsEnabled = !Logging.GetIsLogging();
        }

        public void UpdateUserSettings()
        {
            // ToDo: Load DB values
        }

        #endregion

        #region Connection Related

        private void btnDisconnect_Clicked(object sender, EventArgs e)
        {
            if (!isTimerRun)
                return;

            isTimerRun = false;

            if (Logging.GetIsLogging())
                StopLogging();

            Bluetooth.SetBluetoothState(Bluetooth.BLUETOOTH_STATE.DISCONNECTED);
            UpdateControls();
        }

        private void btnConnect_Clicked(object sender, EventArgs e)
        {
            if (isTimerRun)
                return;

            Bluetooth.SetBluetoothState(Bluetooth.BLUETOOTH_STATE.CONNECTED);
            UpdateControls();
            // ToDo: pass user settings db values
            OpenBluetooth("OBDII", "00:1D:A5:05:4F:05", true);
        }

        #endregion

        #region Processing

        public void RunProcesses()
        {
            isTimerRun = true;

            if (oUserSetting.GetLoggingAuto())
            {
                StartLogging();
            }

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
                            RunProcess(bcmd, oBluetooth);

                            bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
                            //Debug.WriteLine("Process:" + bcmd.Name);
                        }
                    }
                    return isTimerRun;
                }
            );
        }

        private static void RunProcess(BluetoothCmd bcmd, Bluetooth oBluetooth)
        {
            if (bcmd.CmdBytes != null && bcmd.CmdBytes.Length != 0)
            {
                if (!oBluetooth.isTestMode())
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

        #region Logging

        private void btnLogOn_Clicked(object sender, EventArgs e)
        {
            if (!isTimerRun || Logging.GetIsLogging() || Logging.CheckError()) 
                return;

            StartLogging();
        }

        private void btnLogOff_Clicked(object sender, EventArgs e)
        {
            if (!isTimerRun || !Logging.GetIsLogging())
                return;

            StopLogging();
        }

        private void btnLogList_Clicked(object sender, EventArgs e)
        {
            if (isPickerLogsActive)
                return;

            LoadLogPicker();
        }

        private void LoadLogPicker()
        {
            List<string> files = new List<string>();
            
            isPickerLogsActive = true;
            pkrLogs.IsEnabled = true;
            pkrLogs.IsVisible = true;
            pkrLogs.SelectedItem = null;
            pkrLogs.Focus();

            // ToDo: Picker does not display list on first try due to asynchronous/UI threading discrepancy 
            Task.Run(async () =>
            {
                files = await Logging.GetLogFiles();
            }).Wait(1000);

            pkrLogs.ItemsSource = files;
        }

        private void pkrLogs_Unfocused(object sender, FocusEventArgs e)
        {
            isPickerLogsActive = false;
            pkrLogs.IsEnabled = false;
            pkrLogs.IsVisible = false;
        }

        private void StartLogging()
        {
            if (oLogging == null)
                oLogging = new Logging(lblLogFile);

            if (oLogging.InitLogging(oBlueToothCmds))
            {
                oLogging.RunLogging();
                SetLoggingButtons();
            }
        }

        private void SetLoggingButtons()
        {
            if (Logging.GetIsLogging())
            {
                btnLogOff.IsEnabled = true;
                btnLogOn.IsEnabled = false;
            }
            else
            {
                btnLogOff.IsEnabled = false;
                btnLogOn.IsEnabled = true;
            }
        }

        private void StopLogging()
        {
            Logging.StopLogging();
            SetLoggingButtons();
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

        #region Picker Related

        private void btnSelect_Clicked(object sender, EventArgs e)
        {
            if (isTimerRun || isPickerProcessActive)
                return;

            LoadPicker();
        }

        private void SetPickerSelection()
        {
            bool isUserDevice = oUserSetting.isUserDevice();
        }

        private void LoadPicker()
        {
            isPickerProcessActive = true;

            if (oBlueToothCmds_Picker == null)
                oBlueToothCmds_Picker = new BlueToothCmds();

            oBlueToothCmds_Picker.Clear();
            
            try
            {
                // ToDo: Load from db

                oBlueToothCmds_Picker.CreateTestCommands(oUserSetting.GetUserUnits(), false);
                oBlueToothCmds_Picker.RemoveAll(x => x.isSelected);
                pkrProcess.ItemsSource = oBlueToothCmds_Picker;
                pkrProcess.IsEnabled = true;
                pkrProcess.IsVisible = true;
                pkrProcess.SelectedItem = null;
                pkrProcess.Focus();
            }
            catch (Exception ex)
            {
                DisplayMessage(ex.Message);
                isPickerProcessActive = false;
            }
        }

        private void pkrProcess_Unfocused(object sender, FocusEventArgs e)
        {
            isPickerProcessActive = false;
            pkrProcess.IsEnabled = false;
            pkrProcess.IsVisible = false;
        }

        private void pkrProcess_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (CheckPickerSelection()) return;
                SetPickerSelection(((Picker)sender).SelectedIndex);
                isPickerProcessSelected = true;
            }
            catch (Exception ex)
            {
                DisplayMessage(ex.Message);
            }
        }

        private void SetPickerSelection(int index)
        {
            if (index == -1 || pkrProcess.Items.Count == 0)
                return;
        }

        private bool CheckPickerSelection()
        {
            isPickerProcessSelected = !isPickerProcessSelected;
            return !isPickerProcessSelected;
        }

        #endregion

        #region Bluetooth Connection Related

        /// <summary>
        /// Performs Bluetooth connection operations
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="isTest"></param>

        public enum ERROR_TYPE
        {
            NONE,
            ADAPTER_ERROR,
            ADAPTER_DISABLED,
            PAIR_ERROR,
            PAIR_NONE,
            PAIR_FAILED
        }

        private async void OpenBluetooth(string name, string address, bool isTest)
        {
            //oBluetooth = null;
            //oBluetooth = new Bluetooth(true, isTest); // Create connection object

            if (!oBluetooth.isTestMode())
            {
                if (!Bluetooth.CheckAdapterPresent()) // Check if bluetooth is available on this device: display message and return on failure
                {
                    ProcessConnectionError(ERROR_TYPE.ADAPTER_ERROR);
                    return;
                }

                if (!Bluetooth.CheckAdapterEnabled()) // Check if bluetooth is enabled on this device: display message and return on failure
                {
                    ProcessConnectionError(ERROR_TYPE.ADAPTER_DISABLED);
                    return;
                }

                if (!oBluetooth.LoadPairedDevices()) // Attempt to load paired devices: display message and return on failure
                {
                    ProcessConnectionError(ERROR_TYPE.PAIR_ERROR);
                    return;
                }

                if (!oBluetooth.CheckPairedDevices()) // Check if there are paired devices available: display message and return on failure
                {
                    ProcessConnectionError(ERROR_TYPE.PAIR_NONE);
                    return;
                }

                if (!await oBluetooth.OpenPairedDevice(name, address, true)) // Attempt to open paired device: if failed get list of paired devices
                {
                    ProcessConnectionError(ERROR_TYPE.PAIR_FAILED);
                    return;
                }
            }

            // Load some test commands and run processing loop

            oBlueToothCmds = null;
            oBlueToothCmds = new BlueToothCmds();

            // ToDo: replace with db values
            oBlueToothCmds.CreateTestCommands(oUserSetting.GetUserUnits(), true);

            InitListViewItems(oBlueToothCmds);
            RunProcesses();
        }

        private void ProcessConnectionError(ERROR_TYPE Error_Type)
        {
            DisplayMessage(Bluetooth.GetStatusMessage());

            // Todo: add error handling
            switch (Error_Type)
            {
                case ERROR_TYPE.ADAPTER_ERROR: // Fatal Bluetooth not present or device error
                    break;
                case ERROR_TYPE.ADAPTER_DISABLED: // Bluetooth turned off

                    break;
                case ERROR_TYPE.PAIR_ERROR:
                    break;
                case ERROR_TYPE.PAIR_NONE:
                    break;
                case ERROR_TYPE.PAIR_FAILED:
                    break;
                case ERROR_TYPE.NONE:
                    break;
            }

            Bluetooth.SetBluetoothState(Bluetooth.BLUETOOTH_STATE.CONNECTED);
        }

        private async void DisplayMessage(string message)
        {
            await DisplayAlert("Alert", message, "OK");
        }

        #endregion
    }
}
