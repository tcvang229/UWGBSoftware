using System;
using System.Collections.Generic;
using M.OBD._2;
using M.OBD2;
using Xamarin.Forms;

namespace M.OBD
{
    public partial class ProfilePage : ContentPage, IPageLoad
    {
        #region Declarations

        private readonly Bluetooth oBluetooth;
        private readonly UserSetting oUserSetting;

        #endregion

        #region Page Initialization and Update

        public ProfilePage()
        {
            InitializeComponent();

            InitBluetooth(out oBluetooth);
            InitUserSettings(out oUserSetting);
            InitUserSettings();
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
            Appearing += Page_Appearing;

        }

        public void InitUserSettings()
        {

        }

        public void Page_Appearing(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateUserSettings();
        }

        public void UpdateControls()
        {
            IsEnabled = Bluetooth.isBluetoothDisconnected();
        }

        public void UpdateUserSettings()
        {

        }

        #endregion
    }
}
