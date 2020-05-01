using M.OBD._2;
using M.OBD2;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace M.OBD
{
    public partial class DtcPage : ContentPage, IPageLoad
    {
        #region Declarations

        private readonly Bluetooth oBluetooth;
        private readonly UserSettings oUserSettings;

        private bool isErrors;

        #endregion

        #region Page Initialization and Update

        public DtcPage()
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
        }

        public void InitControls()
        {
            Appearing += Page_Appearing;
            btnClear.Clicked += btnClear_Clicked;
        }

        public void Page_Appearing(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateUserSettings();
        }

        public void UpdateControls()
        {
            btnClear.IsEnabled = !Bluetooth.isBluetoothDisconnected();
        }

        public void UpdateUserSettings()
        {

        }

        #endregion

        private void btnClear_Clicked(object sender, EventArgs e)
        {
            if (!isErrors)
                return;

            // ToDo: call clear routines
        }
    }
}