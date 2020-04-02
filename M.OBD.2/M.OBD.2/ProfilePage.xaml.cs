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

        #endregion

        #region Page Initialization and Update

        public ProfilePage()
        {
            InitializeComponent();

            InitBluetooth(out oBluetooth);
            InitUserSettings();
            InitControls();
        }

        public void InitBluetooth(out Bluetooth bluetooth)
        {
            bluetooth = App.GetBluetooth();
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

        }

        public void UpdateUserSettings()
        {

        }

        #endregion
    }
}
