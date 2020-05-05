﻿#region Using statements

using M.OBD._2;
using M.OBD2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
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
        private readonly UserSettings oUserSettings;

        private bool isTimerRun;
        private bool isTestMode;
        private bool isPickerProcessActive;
        private bool isPickerProcessSelected;
        private bool isPickerProcessAdd;
        private bool isPickerLogsActive;
        private bool isPickerLogsSelected;
        private bool isConnect;
        private static bool isDone;
        private UserSettings.UNIT_TYPE UnitType_Last;
        private const int TIMER_UPDATE = 25;       // Update timer iteration delay in ms

        #endregion

        #region Page Initialization and Update

        public ResultsPage()
        {
            InitializeComponent();

            InitBluetooth(out oBluetooth);
            InitUserSettings(out oUserSettings);
            InitControls();
        }

        public void InitBluetooth(out Bluetooth bluetooth)
        {
            bluetooth = App.GetBluetooth();
        }

        public void InitUserSettings(out UserSettings usersettings)
        {
            usersettings = App.GetUserSettings();
            UnitType_Last = UserSettings.UNIT_TYPE.NONE;
        }

        public void InitControls()
        {
            btnConnect.Clicked += btnConnect_Clicked;
            btnDisconnect.Clicked += btnDisconnect_Clicked;
            btnLogOn.Clicked += btnLogOn_Clicked;
            btnLogOff.Clicked += btnLogOff_Clicked;
            btnLogList.Clicked += btnLogList_Clicked;
            btnAdd.Clicked += btnAdd_Clicked;
            btnDel.Clicked += btnDel_Clicked;
            Appearing += Page_Appearing;
            pkrProcess.SelectedIndexChanged += pkrProcess_SelectedIndexChanged;
            pkrProcess.Unfocused += pkrProcess_Unfocused;
            pkrLogs.SelectedIndexChanged += pkrLogs_SelectedIndexChanged;
            pkrLogs.Unfocused += pkrLogs_Unfocused;
            InitListView();
        }

        public void Page_Appearing(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateUserSettings();

            // Call listview generation only if units have changed
            if (oUserSettings.GetUserUnits() != UnitType_Last)
            {
                UnitType_Last = oUserSettings.GetUserUnits();
                UpdateListViewItems();
            }
        }

        public void UpdateControls()
        {
            btnConnect.IsEnabled = Bluetooth.isBluetoothDisconnected();
            btnDisconnect.IsEnabled = !Bluetooth.isBluetoothDisconnected();
            btnAdd.IsEnabled = Bluetooth.isBluetoothDisconnected();
            btnDel.IsEnabled = Bluetooth.isBluetoothDisconnected();
            btnLogOn.IsEnabled = !Bluetooth.isBluetoothDisconnected() && !Logging.GetIsLogging();
            btnLogOff.IsEnabled = !Bluetooth.isBluetoothDisconnected() && Logging.GetIsLogging();
            btnLogList.IsEnabled = Bluetooth.isBluetoothDisconnected();
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

            oBluetooth.CloseConnection();

            Bluetooth.SetBluetoothState(Bluetooth.BLUETOOTH_STATE.DISCONNECTED);
            UpdateControls();
        }

        private void btnConnect_Clicked(object sender, EventArgs e)
        {
            if (isTimerRun || isConnect)
                return;

            OpenConnection();
        }

        private async void OpenConnection()
        {
            btnConnect.IsEnabled = false;
            isConnect = true;

            if (!await OpenBluetooth(oUserSettings.GetDeviceName(), oUserSettings.GetDeviceAddress(),
                oUserSettings.GetIsTestMode()))
            {
                btnConnect.IsEnabled = true;
            }
            else
            {
                Bluetooth.SetBluetoothState(Bluetooth.BLUETOOTH_STATE.CONNECTED);
                UpdateControls();
            }
            isConnect = false;
        }

        #endregion

        #region Processing

        public void RunProcesses()
        {
            isTimerRun = true;
            isDone = true;
            isTestMode = oUserSettings.GetIsTestMode();

            if (oUserSettings.GetLoggingAuto())
            {
                StartLogging();
            }

            DateTime dtCurrent = DateTime.UtcNow;

            foreach (BluetoothCmd bcmd in oBlueToothCmds.Where(x=>x.isProcess))
            {
                bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
                UpdateProcessItem(bcmd);
            }

            Device.StartTimer
            (
                TimeSpan.FromMilliseconds(TIMER_UPDATE), () =>
                {
                    if (isDone)
                    {
                        Run();
                    }

                    return isTimerRun;
                }
            );
        }

        private void Run()
        {
            Task.Run(async () =>
            {
                isDone = false;

                DateTime dtCurrent;

                foreach (BluetoothCmd bcmd in oBlueToothCmds.Where(x => x.isProcess))
                {
                    dtCurrent = DateTime.UtcNow;

                    if (dtCurrent >= bcmd.dtNext)
                    {
                        bcmd.dtNext = dtCurrent.AddMilliseconds(bcmd.Rate);
                        await RunProcess(bcmd, oBluetooth, isTestMode);
                    }
                }
                isDone = true;
            });
        }

        private static async Task RunProcess(BluetoothCmd bcmd, Bluetooth oBluetooth, bool isTest)
        {
            if (bcmd.CmdBytes != null && bcmd.CmdBytes.Length != 0)
            {
                if (!isTest)
                {
                    if (await oBluetooth.SendCommandAsync(bcmd))
                        UpdateProcessItem(bcmd);
                    else
                        Debug.WriteLine("{0} Error: {1}", bcmd.Name, Bluetooth.GetStatusMessage());
                }
                else
                {
                    if (oBluetooth.SendCommandAsync_Test(bcmd))
                        UpdateProcessItem(bcmd);
                    else
                        Debug.WriteLine("{0} Error: {1}", bcmd.Name, Bluetooth.GetStatusMessage());
                }
            }
        }

        private static void UpdateProcessItem(BluetoothCmd bcmd)
        {
            // ToDo: Add direct ProcessItem reference to BluetoothCmd objects for faster iteration?

            foreach (ProcessItem pi in ProcessItems.Where(pi => pi.id == bcmd.Id))
            {
                pi.Value = bcmd.value.ToString(bcmd.GetFormatter());
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

        private async void LoadLogPicker()
        {
            isPickerLogsActive = true;
            pkrLogs.Title = "View Log";

            try
            {
                List<string>  files = await Logging.GetLogFiles();

                if (files != null)
                {
                    pkrLogs.ItemsSource = null;
                    pkrLogs.ItemsSource = files;
                    pkrLogs.IsEnabled = true;
                    pkrLogs.IsVisible = true;
                    pkrLogs.SelectedItem = null;
                    pkrLogs.Focus();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not load files:" + e.Message);
                isPickerLogsActive = false;
                pkrLogs.IsEnabled = false;
                pkrLogs.IsVisible = false;
            }
        }

        private void pkrLogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (CheckPickerLogsSelection()) return;
                SetPickerLogsSelection(((Picker)sender).SelectedIndex);
                isPickerProcessSelected = true;
            }
            catch (Exception ex)
            {
                DisplayMessage(ex.Message);
                isPickerLogsActive = false;
            }
        }

        private void SetPickerLogsSelection(int index)
        {
            if (index == -1 || pkrLogs.Items.Count == 0)
                return;

            ViewLogFile(index);
        }

        private async void ViewLogFile(int index)
        {
            try
            {
                string fname = pkrLogs.Items[index];

                if (string.IsNullOrEmpty(fname))
                    throw new Exception("Invalid Selection");

                string filepathname = Logging.GetLogFilePath(fname);

                if (string.IsNullOrEmpty(filepathname))
                    throw new Exception("Invalid file path");

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filepathname)
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not view file:" + e.Message);
            }
        }

        private bool CheckPickerLogsSelection()
        {
            isPickerLogsSelected = !isPickerLogsSelected;
            return !isPickerLogsSelected;
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
        }

        private void UpdateListViewItems()
        {
            try
            {
                oBlueToothCmds = null;
                oBlueToothCmds = new BlueToothCmds();
                oBlueToothCmds.RetrieveCommands(oUserSettings.GetUserUnits(), false);
                InitListViewItems(oBlueToothCmds);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error updating listview:" + e.Message);
            }
        }

        private static void InitListViewItems(BlueToothCmds oBthCmds)
        {
            ProcessItems.Clear();

            if (oBthCmds == null)
                return;

            foreach (BluetoothCmd bthCmd in oBthCmds)
            {
                if (bthCmd.isSelected && bthCmd.Selection_Type == BlueToothCmds.SELECTION_TYPE.NONE)
                    bthCmd.Selection_Type = BlueToothCmds.SELECTION_TYPE.USER;

                if (bthCmd.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER ||
                    bthCmd.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER_PROCESS)
                {
                    AddListViewItem(bthCmd);
                }
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

        #region Process Picker Related

        private void btnAdd_Clicked(object sender, EventArgs e)
        {
            if (isTimerRun || isPickerProcessActive)
                return;
            
            LoadProcessPicker(true);
        }

        private void btnDel_Clicked(object sender, EventArgs e)
        {
            if (isTimerRun || isPickerProcessActive)
                return;

            LoadProcessPicker(false);
        }

        private void LoadProcessPicker(bool isAdd)
        {
            isPickerProcessActive = true;
            isPickerProcessAdd = isAdd;

            pkrProcess.Title = (isAdd) ? "Add a Process" : "Delete a Process";

            if (oBlueToothCmds_Picker == null)
                oBlueToothCmds_Picker = new BlueToothCmds();

            oBlueToothCmds_Picker.Clear();
            
            try
            {
                //oBlueToothCmds_Picker.CreateTestCommands(oUserSetting.GetUserUnits(), false);
                oBlueToothCmds_Picker.RetrieveCommands(oUserSettings.GetUserUnits(), false);
                oBlueToothCmds_Picker.RemoveAll(x => (isPickerProcessAdd) ? x.isSelected : !x.isSelected);
                pkrProcess.ItemsSource = null;
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
                if (CheckProcessPickerSelection()) return;
                SetProcessPickerSelection(((Picker)sender).SelectedIndex);
                isPickerProcessSelected = true;
            }
            catch (Exception ex)
            {
                DisplayMessage(ex.Message);
            }
        }

        private void SetProcessPickerSelection(int index)
        {
            if (index == -1 || pkrProcess.Items.Count == 0)
                return;

            BluetoothCmd bthCmd = (BluetoothCmd) pkrProcess.SelectedItem;

            if (bthCmd == null)
                throw new Exception("Selection Error Occurred");

            bthCmd.isSelected = isPickerProcessAdd;

            if (!BlueToothCmds.updateRecord(bthCmd))
                throw  new Exception("Failed to update command");

            UpdateListViewItems();

            if (!bthCmd.isSelected)
            {
                bthCmd.Selection_Type = (bthCmd.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER_PROCESS)
                    ? BlueToothCmds.SELECTION_TYPE.PROCESS
                    : BlueToothCmds.SELECTION_TYPE.NONE;
            }
            else
            {
                bthCmd.Selection_Type = (bthCmd.Selection_Type == BlueToothCmds.SELECTION_TYPE.PROCESS)
                    ? BlueToothCmds.SELECTION_TYPE.USER_PROCESS
                    : BlueToothCmds.SELECTION_TYPE.USER;
            }
        }

        private bool CheckProcessPickerSelection()
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

        private async Task<bool> OpenBluetooth(string name, string address, bool isTest)
        {
            //oBluetooth = null;
            //oBluetooth = new Bluetooth(true, isTest); // Create connection object

            if (!oUserSettings.GetIsTestMode())
            {
                if (!Bluetooth.CheckAdapterPresent()) // Check if bluetooth is available on this device: display message and return on failure
                {
                    ProcessConnectionError(ERROR_TYPE.ADAPTER_ERROR);
                    return false;
                }

                if (!Bluetooth.CheckAdapterEnabled()) // Check if bluetooth is enabled on this device: display message and return on failure
                {
                    ProcessConnectionError(ERROR_TYPE.ADAPTER_DISABLED);
                    return false;
                }

                if (!oBluetooth.LoadPairedDevices()) // Attempt to load paired devices: display message and return on failure
                {
                    ProcessConnectionError(ERROR_TYPE.PAIR_ERROR);
                    return false;
                }

                if (!oBluetooth.CheckPairedDevices()) // Check if there are paired devices available: display message and return on failure
                {
                    ProcessConnectionError(ERROR_TYPE.PAIR_NONE);
                    return false;
                }

                if (!await oBluetooth.OpenPairedDevice(name, address, true)) // Attempt to open paired device: if failed get list of paired devices
                {
                    ProcessConnectionError(ERROR_TYPE.PAIR_FAILED);
                    return false;
                }
            }

            // Load commands and run processing loop

            oBlueToothCmds = null;
            oBlueToothCmds = new BlueToothCmds();

            // ToDo: replace with db values
            //oBlueToothCmds.CreateTestCommands(oUserSetting.GetUserUnits(), true);
            oBlueToothCmds.RetrieveCommands(oUserSettings.GetUserUnits(), true);

            InitCommands(oBlueToothCmds);
            InitListViewItems(oBlueToothCmds);
            RunProcesses();
            return true;
        }

        private void InitCommands(BlueToothCmds oBthCmds)
        {
            if (oBthCmds == null)
                return;

            foreach (BluetoothCmd bthCmd in oBthCmds)
            {
                if (bthCmd.isSelected && bthCmd.Selection_Type == BlueToothCmds.SELECTION_TYPE.NONE)
                    bthCmd.Selection_Type = BlueToothCmds.SELECTION_TYPE.USER;

                bthCmd.isProcess = (bthCmd.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER ||
                                    bthCmd.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER_PROCESS);
            }
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
