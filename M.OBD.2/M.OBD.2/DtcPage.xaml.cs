using M.OBD._2;
using M.OBD2;
using System;
using Xamarin.Forms;

namespace M.OBD
{

    public partial class DtcPage : ContentPage, IPageLoad
    {
        #region Declarations

        private readonly Bluetooth oBluetooth;
        private readonly UserSetting oUserSetting;

        #endregion

        #region Page Initialization and Update

        public DtcPage()
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

        }
    }
}