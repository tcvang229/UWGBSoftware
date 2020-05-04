#region Using Statements
using M.OBD._2;
using M.OBD2;
using SQLite;
using System;
using Xamarin.Forms;
#endregion

namespace M.OBD
{
    public partial class ProfilePage : ContentPage, IPageLoad
    {
        #region Declarations

        private readonly Bluetooth oBluetooth;
        private readonly UserSettings oUserSettings;

        private bool isChanged;

        #endregion

        #region Page Initialization and Update

        public ProfilePage()
        {
            InitializeComponent();
            getCurrentMetricSetting();

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
        }

        public void InitControls()
        {
            Appearing += Page_Appearing;

            UpdateCheckBoxes();
            chkImperial.CheckedChanged += chkImperial_CheckedChanged;
            chkMetric.CheckedChanged += chkMetric_CheckedChanged;
            chkLoggingAuto.CheckedChanged += chkLoggingAuto_CheckedChanged;
            chkTestMode.CheckedChanged += chkTestMode_CheckedChanged;
            btnSave.Clicked += btnSave_Clicked;
            btnCancel.Clicked += btnCancel_Clicked;
            UpdateChangedState(false);
        }

        public void Page_Appearing(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateUserSettings();
        }

        public void UpdateControls()
        {
            IsEnabled = Bluetooth.isBluetoothDisconnected();

            UpdateCheckBoxes();
        }

        public void UpdateUserSettings()
        {

        }

        #endregion

        #region DB

        //get the current metric setting from the DB
        //then set the value in the UI
        public void getCurrentMetricSetting()
        {
            //connect to the DB
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {
                //get the entire table
                var userSettings = connection.Table<UserSetting>();

                // ToDo: temporary bypass
                //get the first entry in the table
                //bool isMetric = userSettings.FirstOrDefault().GetMetricUnits();
                //bool isMetric = false;

                //if the user has saved metric as a preference
                //update the check boxes in the UI
                //if (isMetric)
                //{
                //    chkMetric.IsChecked = true;
                //    chkImperial.IsChecked = false;
                //}
                //else
                //{
                //    chkMetric.IsChecked = false;
                //    chkImperial.IsChecked = true;
                //}
            }
        }

        #endregion

        #region Check Boxes

        private void UpdateCheckBoxes()
        {
            chkImperial.IsChecked = !oUserSettings.GetIsMetric();
            chkMetric.IsChecked = oUserSettings.GetIsMetric();
            chkLoggingAuto.IsChecked = oUserSettings.GetLoggingAuto();
            chkTestMode.IsChecked = oUserSettings.GetIsTestMode();
        }

        private void chkLoggingAuto_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateChangedState(true);
        }

        private void chkMetric_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (chkMetric.IsChecked)
            {
                chkImperial.IsChecked = false;
                oUserSettings.SetIsMetric(true);
                oUserSettings.SetUnitType(UserSettings.UNIT_TYPE.METRIC);
            }

            UpdateChangedState(true);
        }

        private void chkImperial_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (chkImperial.IsChecked)
            {
                chkMetric.IsChecked = false;
                oUserSettings.SetIsMetric(false);
                oUserSettings.SetUnitType(UserSettings.UNIT_TYPE.IMPERIAL);
            }

            UpdateChangedState(true);
        }

        private void chkTestMode_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateChangedState(true);
            oUserSettings.SetIsTestMode(chkTestMode.IsChecked);
        }

        #endregion

        #region Save/Cancel

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            if (isChanged)
                UpdateChangedState(false);
        }

        private void btnSave_Clicked(object sender, EventArgs e)
        {
            if (isChanged)
                UpdateChangedState(false);
        }

        private void UpdateChangedState(bool state)
        {
            isChanged = state;

            btnSave.IsEnabled = isChanged;
            btnCancel.IsEnabled = isChanged;
        }

        #endregion
    }
}
