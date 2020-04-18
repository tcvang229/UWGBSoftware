using M.OBD._2;
using M.OBD2;
using SQLite;
using System;
using Xamarin.Forms;

namespace M.OBD
{
    public partial class ProfilePage : ContentPage, IPageLoad
    {
        #region Declarations

        private readonly Bluetooth oBluetooth;
        private readonly UserSetting oUserSetting;

        private bool isChanged;

        #endregion

        #region Page Initialization and Update

        public ProfilePage()
        {
            InitializeComponent();
            getCurrentMetricSetting();

            InitBluetooth(out oBluetooth);
            InitUserSettings(out oUserSetting);
            InitControls();
        }

        //get the current metric setting from the DB
        //then set the value in the UI
        public void getCurrentMetricSetting()
        {
            //connect to the DB
            using (SQLiteConnection connection = new SQLiteConnection(App.Database))
            {

                //get the entire table
                var userSettings = connection.Table<UserSetting>();

                //get the first entry in the table
                bool isMetric = userSettings.FirstOrDefault().GetMetricUnits();

                //if the user has saved metric as a preference
                //update the check boxes in the UI
                if (isMetric)
                {
                    chkMetric.IsChecked = true;
                    chkImperial.IsChecked = false;
                } else
                {
                    chkMetric.IsChecked = false;
                    chkImperial.IsChecked = true;
                }
            }

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
            Appearing += Page_Appearing;

            UpdateUnitCheckBoxes();
            chkImperial.CheckedChanged += chkImperial_CheckedChanged;
            chkMetric.CheckedChanged += chkMetric_CheckedChanged;
            chkLoggingEnabled.CheckedChanged += chkLoggingEnabled_CheckedChanged;
            chkLoggingAuto.CheckedChanged += chkLoggingAuto_CheckedChanged;
            btnSave.Clicked += btnSave_Clicked;
            btnCancel.Clicked += btnCancel_Clicked;
            UpdateChangedState(false);
        }

        private void chkLoggingAuto_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateChangedState(true);
        }

        private void chkLoggingEnabled_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateChangedState(true);
        }

        private void chkMetric_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (chkMetric.IsChecked)
                chkImperial.IsChecked = false;

            UpdateChangedState(true);
        }

        private void chkImperial_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (chkImperial.IsChecked)
                chkMetric.IsChecked = false;

            UpdateChangedState(true);
        }

        public void Page_Appearing(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateUserSettings();
        }

        public void UpdateControls()
        {
            IsEnabled = Bluetooth.isBluetoothDisconnected();

            UpdateUnitCheckBoxes();
        }

        public void UpdateUserSettings()
        {

        }

        #endregion

        #region Check Boxes

        private void UpdateUnitCheckBoxes()
        {
            chkImperial.IsChecked = oUserSetting.GetImperialUnits();
            chkMetric.IsChecked = !oUserSetting.GetImperialUnits();
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
